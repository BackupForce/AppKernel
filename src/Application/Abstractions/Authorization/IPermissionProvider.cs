namespace Application.Abstractions.Authorization;

public interface IPermissionProvider
{
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, Guid? nodeId, Guid? tenantId);

    Task<IReadOnlyList<string>> GetAllowedPermissionCodesAsync(
        Guid userId,
        Guid tenantId,
        Guid? nodeId,
        CancellationToken cancellationToken);
}
