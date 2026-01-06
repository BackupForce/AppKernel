using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(int roleId, bool includePermissions, CancellationToken cancellationToken)
    {
        IQueryable<Role> query = context.Set<Role>().AsQueryable();

        if (includePermissions)
        {
            query = query.Include(role => role.Permissions);
        }

        return query.FirstOrDefaultAsync(role => role.Id == roleId, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(
    string name,
    int? excludingRoleId,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        string candidateName = name.Trim();

        IQueryable<Role> query = context.Set<Role>();

        if (excludingRoleId.HasValue)
        {
            int excludedId = excludingRoleId.Value;

            bool exists = await query.AnyAsync(
                role =>
                    role.Id != excludedId &&
                    string.Equals(role.Name, candidateName, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

            return !exists;
        }
        else
        {
            bool exists = await query.AnyAsync(
                role => string.Equals(role.Name, candidateName, StringComparison.OrdinalIgnoreCase),
                cancellationToken);

            return !exists;
        }
    }

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken)
    {
        List<Role> roles = await context.Set<Role>()
            .Include(role => role.Permissions)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken)
    {
        List<Permission> permissions = await context.Set<Permission>()
            .Where(permission => permission.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return permissions;
    }

    public void Insert(Role role)
    {
        context.Set<Role>().Add(role);
    }

    public void Remove(Role role)
    {
        context.Set<Role>().Remove(role);
    }

    public Task AddPermissionsAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken)
    {
        var permissionList = permissions.ToList();

        return context.Set<Permission>().AddRangeAsync(permissionList, cancellationToken);
    }

    public async Task RemovePermissionsAsync(
    int roleId,
    IEnumerable<string> permissionCodes,
    CancellationToken cancellationToken)
    {
        if (permissionCodes is null)
        {
            return;
        }

        var targets = permissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (targets.Count == 0)
        {
            return;
        }

        List<Permission> permissionsForRole = await context.Set<Permission>()
            .Where(permission => permission.RoleId == roleId)
            .ToListAsync(cancellationToken);

        if (permissionsForRole.Count == 0)
        {
            return;
        }

        var toRemove = permissionsForRole
            .Where(permission => targets.Contains(permission.Name))
            .ToList();

        if (toRemove.Count == 0)
        {
            return;
        }

        context.Set<Permission>().RemoveRange(toRemove);
    }



    public async Task RemovePermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken)
    {
        List<Permission> permissions = await context.Set<Permission>()
            .Where(permission => permission.RoleId == roleId)
            .ToListAsync(cancellationToken);

        if (permissions.Count == 0)
        {
            return;
        }

        context.Set<Permission>().RemoveRange(permissions);
    }
}
