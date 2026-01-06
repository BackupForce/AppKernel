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

    public Task<bool> IsNameUniqueAsync(string name, int? excludingRoleId, CancellationToken cancellationToken)
    {
        string normalizedName = name.ToLowerInvariant();
        IQueryable<Role> query = context.Set<Role>();

        if (excludingRoleId.HasValue)
        {
            int excludedId = excludingRoleId.Value;
            return query.AllAsync(
                role => role.Id == excludedId || role.Name.ToLower() != normalizedName,
                cancellationToken);
        }

        return query.AllAsync(role => role.Name.ToLower() != normalizedName, cancellationToken);
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
        List<Permission> permissionList = permissions.ToList();

        return context.Set<Permission>().AddRangeAsync(permissionList, cancellationToken);
    }

    public async Task RemovePermissionsAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken)
    {
        List<string> permissionCodeList = permissionCodes
            .Select(code => code.ToLowerInvariant())
            .ToList();

        if (permissionCodeList.Count == 0)
        {
            return;
        }

        List<Permission> permissions = await context.Set<Permission>()
            .Where(permission =>
                permission.RoleId == roleId && permissionCodeList.Contains(permission.Name.ToLower()))
            .ToListAsync(cancellationToken);

        if (permissions.Count == 0)
        {
            return;
        }

        context.Set<Permission>().RemoveRange(permissions);
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
