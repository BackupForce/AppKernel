namespace Application.Abstractions.Authorization;

public interface IPermissionProvider
{
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, Guid? nodeId);
}
