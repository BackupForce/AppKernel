using Application.Abstractions.Authentication;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeders;

public sealed class TenantRootUserSeeder : IDataSeeder
{
    private const string TenantAdminRoleName = "TENANT_ADMIN";
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

        User? existing = await _db.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(u => u.Email == tenantRootEmail);
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
        }
        else if (existing.Type != UserType.Tenant || existing.TenantId != tenant.Id)
        {
            // 中文註解：修正既有帳號型別與租戶綁定，避免錯誤分流。
            existing.UpdateType(UserType.Tenant, tenant.Id);
            await _db.SaveChangesAsync();
        }

        await EnsureTenantAdminRoleBindingAsync(existing, tenant.Id);

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

    private async Task EnsureTenantAdminRoleBindingAsync(User user, Guid tenantId)
    {
        if (!await SeederSchemaGuard.HasColumnAsync(_db, "Roles", "TenantId", _logger))
        {
            // TODO: 中文註解：若資料表尚未加入 Role.TenantId 欄位，先略過角色種子流程。
            _logger.LogWarning("⚠️ Roles.TenantId 欄位尚未準備，略過租戶角色建立與指派。");
            return;
        }

        Role? role = await EnsureTenantAdminRoleAsync(tenantId);
        if (role is null)
        {
            return;
        }

        if (user.HasRole(role.Id))
        {
            return;
        }

        user.AssignRole(role);
        await _db.SaveChangesAsync();
    }

    private async Task<Role?> EnsureTenantAdminRoleAsync(Guid tenantId)
    {
        string normalizedRoleName = TenantAdminRoleName.Trim().ToUpperInvariant();
        Role? role = await _db.Set<Role>()
            .AsTracking()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId
                && r.Name != null
                && r.Name.Trim().Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            role = Role.Create(TenantAdminRoleName, tenantId);
            _db.Set<Role>().Add(role);
            await _db.SaveChangesAsync();
        }

        await EnsureRolePermissionsAsync(role, PermissionScope.Tenant);

        return role;
    }

    private async Task EnsureRolePermissionsAsync(Role role, PermissionScope scope)
    {
        List<Permission> existingPermissions = await _db.Set<Permission>()
            .Where(permission => permission.RoleId == role.Id)
            .ToListAsync();

        HashSet<string> existingCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (Permission permission in existingPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            existingCodes.Add(permission.Name.Trim().ToUpperInvariant());
        }

        List<string> expectedCodes = PermissionCatalog.AllPermissionCodes
            .Where(code =>
                PermissionCatalog.TryGetScope(code, out PermissionScope resolvedScope)
                && resolvedScope == scope)
            .Select(code => code.Trim().ToUpperInvariant())
            .ToList();

        List<Permission> toAdd = new List<Permission>();
        List<Permission> toRemove = new List<Permission>();
        foreach (string code in expectedCodes)
        {
            if (existingCodes.Contains(code))
            {
                continue;
            }

            Permission permission = Permission.CreateForRole(code, code, role.Id);
            toAdd.Add(permission);
        }

        foreach (Permission permission in existingPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            string normalizedName = permission.Name.Trim().ToUpperInvariant();
            if (!expectedCodes.Contains(normalizedName))
            {
                toRemove.Add(permission);
            }
        }

        if (toAdd.Count == 0 && toRemove.Count == 0)
        {
            return;
        }

        if (toRemove.Count > 0)
        {
            _db.Set<Permission>().RemoveRange(toRemove);
        }

        if (toAdd.Count > 0)
        {
            await _db.Set<Permission>().AddRangeAsync(toAdd);
        }

        await _db.SaveChangesAsync();
    }
}
