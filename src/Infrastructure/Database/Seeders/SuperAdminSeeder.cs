using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        IReadOnlyCollection<string> allPermissionCodes = PermissionCatalog.AllPermissionCodes;

        Role? role = await _db.Set<Role>()
            .FirstOrDefaultAsync(
                r => string.Equals(r.Name, _options.RoleName, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

        if (role is null)
        {
            role = Role.Create(_options.RoleName);
            _db.Set<Role>().Add(role);
            await _db.SaveChangesAsync(cancellationToken);
        }

        List<Permission> existingPermissions = await _db.Set<Permission>()
            .Where(permission => permission.RoleId == role.Id)
            .ToListAsync(cancellationToken);

        var existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Permission permission in existingPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            existingCodes.Add(permission.Name);
        }

        var permissionsToAdd = new List<Permission>();
        foreach (string code in allPermissionCodes)
        {
            if (existingCodes.Contains(code))
            {
                continue;
            }

            var permissionToAdd = Permission.CreateForRole(code, code, role.Id);
            permissionsToAdd.Add(permissionToAdd);
        }

        if (permissionsToAdd.Count > 0)
        {
            await _db.Set<Permission>().AddRangeAsync(permissionsToAdd, cancellationToken);
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
