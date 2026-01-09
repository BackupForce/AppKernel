using Application.Abstractions.Authentication;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel.Identity;

namespace Infrastructure.Database.Seeders;
public class RootUserSeeder : IDataSeeder
{
    private const string PlatformAdminRoleName = "PLATFORM_ADMIN";
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;
    private readonly ILogger<RootUserSeeder> _logger;

    public RootUserSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration config,
        ILogger<RootUserSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        string email = _config["RootUser:Email"] ?? RootUser.DefaultEmail;
        string? password = _config["RootUser:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("⚠️ RootUser password not provided. Skipping root seeding.");
            return;
        }

        Email rootEmail = Email.Create(email).Value;

        User? existing = await _db.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(u => u.Email == rootEmail);
        if (existing != null)
        {
            if (existing.Type != UserType.Platform || existing.TenantId.HasValue)
            {
                // 中文註解：確保既有 root 帳號修正為平台使用者，避免租戶污染。
                existing.UpdateType(UserType.Platform, null);
                await _db.SaveChangesAsync();
            }

            await EnsurePlatformAdminRoleBindingAsync(existing);

            _logger.LogInformation("✅ Root user already exists: {Email}", email);
            return;
        }

        User user = User.Create(
            rootEmail,
            new Name("root"),
            _passwordHasher.Hash(password),
            false,
            UserType.Platform,
            null);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await EnsurePlatformAdminRoleBindingAsync(user);

        _logger.LogInformation("🚀 Root user created: {Email}", email);
    }

    private async Task EnsurePlatformAdminRoleBindingAsync(User user)
    {
        if (!await SeederSchemaGuard.HasColumnAsync(_db, "Roles", "TenantId", _logger))
        {
            // TODO: 中文註解：若資料表尚未加入 Role.TenantId 欄位，先略過角色種子流程。
            _logger.LogWarning("⚠️ Roles.TenantId 欄位尚未準備，略過平台角色建立與指派。");
            return;
        }

        Role? role = await EnsurePlatformAdminRoleAsync();
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

    private async Task<Role?> EnsurePlatformAdminRoleAsync()
    {
        string normalizedRoleName = PlatformAdminRoleName.Trim().ToUpperInvariant();
        Role? role = await _db.Set<Role>()
            .AsTracking()
            .FirstOrDefaultAsync(r => r.TenantId == null
                && r.Name != null
                && r.Name.Trim().Equals(normalizedRoleName, StringComparison.OrdinalIgnoreCase));

        if (role is null)
        {
            role = Role.Create(PlatformAdminRoleName, null);
            _db.Set<Role>().Add(role);
            await _db.SaveChangesAsync();
        }

        await EnsureRolePermissionsAsync(role, PermissionScope.Platform);

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
