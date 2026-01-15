using Domain.Security;

namespace Application.Abstractions.Authorization;

public sealed class PermissionCheckContext
{
    public PermissionCheckContext(string permissionCode, PermissionScope scope, Guid? tenantId, Guid? targetUserId)
    {
        PermissionCode = permissionCode;
        Scope = scope;
        TenantId = tenantId;
        TargetUserId = targetUserId;
    }

    public string PermissionCode { get; }
    public PermissionScope Scope { get; }
    public Guid? TenantId { get; }
    public Guid? TargetUserId { get; }
}
