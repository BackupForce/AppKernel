using Application.Abstractions.Authentication;
using Application.Abstractions.Infrastructure;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeders;

public sealed class TenantRootUserSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;
    private readonly ILogger<TenantRootUserSeeder> _logger;

    public TenantRootUserSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration config,
        ILogger<TenantRootUserSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        string? password = _config["TenantRootUser:Password"] ?? _config["RootUser:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("⚠️ TenantRootUser password not provided. Skipping tenant root seeding.");
            return;
        }

        List<Tenant> tenants = await _db.Tenants.AsNoTracking().ToListAsync();
        foreach (Tenant tenant in tenants)
        {
            await EnsureTenantRootUserAsync(tenant, password);
        }
    }

    private async Task EnsureTenantRootUserAsync(Tenant tenant, string password)
    {
        string email = BuildTenantRootEmail(tenant.Code);
        Email tenantRootEmail = Email.Create(email).Value;

        User? existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == tenantRootEmail);
        if (existing is null)
        {
            User user = User.Create(
                tenantRootEmail,
                new Name($"root-{tenant.Code}"),
                _passwordHasher.Hash(password),
                false,
                UserType.Tenant,
                tenant.Id);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            existing = user;
            _logger.LogInformation("✅ Tenant root user created: {Email}", email);

            // TODO: 中文註解：若存在 Tenant 管理角色，請在此指派以確保租戶權限完整。
        }
        else if (existing.Type != UserType.Tenant || existing.TenantId != tenant.Id)
        {
            // 中文註解：修正既有帳號型別與租戶綁定，避免錯誤分流。
            existing.UpdateType(UserType.Tenant, tenant.Id);
            await _db.SaveChangesAsync();
        }

        bool hasTenant = await _db.UserTenants
            .AnyAsync(userTenant => userTenant.UserId == existing.Id && userTenant.TenantId == tenant.Id);
        if (!hasTenant)
        {
            _db.UserTenants.Add(UserTenant.Create(existing.Id, tenant.Id));
            await _db.SaveChangesAsync();
        }
    }

    private static string BuildTenantRootEmail(string tenantCode)
    {
        // 中文註解：使用租戶代碼組合唯一 email，避免跨租戶衝突。
        string normalized = tenantCode.Trim().ToUpperInvariant();
        return $"root+{normalized}@system.local";
    }
}
