using Application.Abstractions.Authorization;
using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

internal sealed class GrantedPermissionProvider : IGrantedPermissionProvider
{
    private readonly ApplicationDbContext _dbContext;

    public GrantedPermissionProvider(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlySet<string>> GetPlatformPermissionsAsync(Guid callerUserId, CancellationToken ct)
    {
        List<int> roleIds = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == callerUserId)
            .SelectMany(user => user.Roles
                .Where(role => role.TenantId == null)
                .Select(role => role.Id))
            .ToListAsync(ct);

        List<string> rolePermissionCodes = await _dbContext.Set<Permission>()
            .AsNoTracking()
            .Where(permission => permission.RoleId.HasValue && roleIds.Contains(permission.RoleId.Value))
            .Select(permission => permission.Name)
            .ToListAsync(ct);

        return NormalizePermissions(rolePermissionCodes);
    }

    public async Task<IReadOnlySet<string>> GetTenantPermissionsAsync(
        Guid callerUserId,
        Guid tenantId,
        CancellationToken ct)
    {
        bool isInTenant = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == callerUserId && user.TenantId == tenantId, ct);

        if (!isInTenant)
        {
            return new HashSet<string>();
        }

        List<int> roleIds = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == callerUserId)
            .SelectMany(user => user.Roles
                .Where(role => role.TenantId == tenantId)
                .Select(role => role.Id))
            .ToListAsync(ct);

        List<Guid> roleSubjectIds = roleIds
            .Select(MapRoleIdToSubjectId)
            .ToList();

        List<Guid> groupIds = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(userGroup => userGroup.UserId == callerUserId)
            .Select(userGroup => userGroup.GroupId)
            .ToListAsync(ct);

        List<string> assignmentPermissions = await _dbContext.PermissionAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == tenantId)
            .Where(assignment => assignment.Decision == Decision.Allow)
            .Where(assignment =>
                assignment.SubjectType == SubjectType.User && assignment.SubjectId == callerUserId
                || assignment.SubjectType == SubjectType.Role && roleSubjectIds.Contains(assignment.SubjectId)
                || assignment.SubjectType == SubjectType.Group && groupIds.Contains(assignment.SubjectId))
            .Select(assignment => assignment.PermissionCode)
            .ToListAsync(ct);

        List<string> rolePermissionCodes = await _dbContext.Set<Permission>()
            .AsNoTracking()
            .Where(permission => permission.RoleId.HasValue && roleIds.Contains(permission.RoleId.Value))
            .Select(permission => permission.Name)
            .ToListAsync(ct);

        HashSet<string> merged = new HashSet<string>();
        AddPermissions(merged, rolePermissionCodes);
        AddPermissions(merged, assignmentPermissions);

        return merged;
    }

    private static void AddPermissions(HashSet<string> target, IEnumerable<string> permissions)
    {
        foreach (string permission in permissions)
        {
            if (string.IsNullOrWhiteSpace(permission))
            {
                continue;
            }

            string normalized = permission.Trim().ToUpperInvariant();
            target.Add(normalized);
        }
    }

    private static HashSet<string> NormalizePermissions(IEnumerable<string> permissions)
    {
        HashSet<string> normalized = new HashSet<string>();
        AddPermissions(normalized, permissions);
        return normalized;
    }

    private static Guid MapRoleIdToSubjectId(int roleId)
    {
        return new Guid(roleId, 0, 0, new byte[8]);
    }
}
