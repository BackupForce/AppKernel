namespace Application.Abstractions.Authorization;

public interface IGrantedPermissionProvider
{
    Task<IReadOnlySet<string>> GetPlatformPermissionsAsync(Guid callerUserId, CancellationToken ct);
    Task<IReadOnlySet<string>> GetTenantPermissionsAsync(Guid callerUserId, Guid tenantId, CancellationToken ct);
}
