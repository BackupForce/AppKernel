namespace Domain.Security;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int roleId, bool includePermissions, CancellationToken cancellationToken);

    Task<bool> IsNameUniqueAsync(string name, Guid? tenantId, int? excludingRoleId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Role>> GetPlatformRolesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Role>> GetTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken);

    Task<Role?> GetByCodeAsync(Guid? tenantId, string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken);

    void Insert(Role role);

    void Remove(Role role);

    Task AddPermissionsAsync(IEnumerable<Permission> permissions, CancellationToken cancellationToken);

    Task RemovePermissionsAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken);

    Task RemovePermissionsByRoleIdAsync(int roleId, CancellationToken cancellationToken);
}
