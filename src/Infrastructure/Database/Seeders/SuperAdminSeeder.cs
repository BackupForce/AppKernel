using Domain.Security;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Database.Seeders;

public sealed class SuperAdminSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SuperAdminSeeder> _logger;
    private readonly SuperAdminSeedOptions _options;

    public SuperAdminSeeder(
        ApplicationDbContext db,
        ILogger<SuperAdminSeeder> logger,
        IOptions<SuperAdminSeedOptions> options)
    {
        _db = db;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            // 中文註解：設定停用時不執行 Seed。
            return;
        }

        if (!await SeederSchemaGuard.HasColumnAsync(_db, "Roles", "TenantId", _logger, cancellationToken))
        {
            // TODO: 中文註解：若資料表尚未加入 Role.TenantId 欄位，先略過平台角色種子流程。
            _logger.LogWarning("⚠️ Roles.TenantId 欄位尚未準備，略過 SuperAdmin 角色建立。");
            return;
        }

        List<string> platformPermissionCodes = PermissionCatalog.AllPermissionCodes
            .Where(code =>
                PermissionCatalog.TryGetScope(code, out PermissionScope scope)
                && scope == PermissionScope.Platform)
            .Select(code => code.Trim().ToUpperInvariant())
            .ToList();

        string normalizedRoleName = _options.RoleName.Trim().ToUpperInvariant();

        Role? role = await _db.Set<Role>()
            .FirstOrDefaultAsync(
                r => r.TenantId == null
                    && r.Name != null
                    && r.Name.Trim().ToUpperInvariant() == normalizedRoleName,
                cancellationToken);


        if (role is null)
        {
            role = Role.Create(_options.RoleName, null);
            _db.Set<Role>().Add(role);
            await _db.SaveChangesAsync(cancellationToken);
        }

        List<Permission> existingPermissions = await _db.Set<Permission>()
            .Where(permission => permission.RoleId == role.Id)
            .ToListAsync(cancellationToken);

        HashSet<string> expectedCodes = new HashSet<string>(platformPermissionCodes, StringComparer.Ordinal);
        HashSet<string> existingCodes = new HashSet<string>(StringComparer.Ordinal);
        bool hasUpdates = false;
        foreach (Permission permission in existingPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            string normalizedName = permission.Name.Trim().ToUpperInvariant();
            if (expectedCodes.Contains(normalizedName) && permission.Name != normalizedName)
            {
                permission.Name = normalizedName;
                hasUpdates = true;
            }

            existingCodes.Add(normalizedName);
        }

        List<Permission> permissionsToAdd = new List<Permission>();
        List<Permission> permissionsToRemove = new List<Permission>();
        foreach (string code in platformPermissionCodes)
        {
            if (existingCodes.Contains(code))
            {
                continue;
            }

            Permission permissionToAdd = Permission.CreateForRole(code, code, role.Id);
            permissionsToAdd.Add(permissionToAdd);
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
                permissionsToRemove.Add(permission);
            }
        }

        if (permissionsToAdd.Count > 0)
        {
            await _db.Set<Permission>().AddRangeAsync(permissionsToAdd, cancellationToken);
            hasUpdates = true;
        }

        if (permissionsToRemove.Count > 0)
        {
            _db.Set<Permission>().RemoveRange(permissionsToRemove);
            hasUpdates = true;
        }

        if (hasUpdates)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        await EnsureUserRoleBindingAsync(role, cancellationToken);
    }

    private async Task EnsureUserRoleBindingAsync(Role role, CancellationToken cancellationToken)
    {
        Result<Email> emailResult = Email.Create(_options.UserEmail);
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Seed 使用者 email 格式無效: {Email}，略過角色綁定", _options.UserEmail);
            return;
        }

        Email seedEmail = emailResult.Value;

        User? user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == seedEmail, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("找不到 seed 使用者 email: {Email}，略過角色綁定", _options.UserEmail);
            return;
        }

        if (user.HasRole(role.Id))
        {
            return;
        }

        user.AssignRole(role);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
