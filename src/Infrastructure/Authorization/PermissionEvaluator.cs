using Application.Abstractions.Authorization;
using Domain.Security;

namespace Infrastructure.Authorization;

public sealed class PermissionEvaluator : IPermissionEvaluator
{
    private readonly IGrantedPermissionProvider _grantedPermissionProvider;

    public PermissionEvaluator(IGrantedPermissionProvider grantedPermissionProvider)
    {
        _grantedPermissionProvider = grantedPermissionProvider;
    }

    public async Task<bool> AuthorizeAsync(
        PermissionRequirement requirement,
        CallerContext callerContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requirement.PermissionCode))
        {
            return false;
        }

        string requiredPermission = NormalizePermissionCode(requirement.PermissionCode);
        IReadOnlySet<string> grantedPermissions;

        switch (requirement.Scope)
        {
            case PermissionScope.Platform:
                grantedPermissions = await _grantedPermissionProvider.GetPlatformPermissionsAsync(
                    callerContext.CallerUserId,
                    cancellationToken);
                break;
            case PermissionScope.Tenant:
                if (!requirement.TenantId.HasValue)
                {
                    return false;
                }

                grantedPermissions = await _grantedPermissionProvider.GetTenantPermissionsAsync(
                    callerContext.CallerUserId,
                    requirement.TenantId.Value,
                    cancellationToken);
                break;
            case PermissionScope.Self:
                if (!requirement.TargetUserId.HasValue)
                {
                    return false;
                }

                if (callerContext.CallerUserId != requirement.TargetUserId.Value)
                {
                    return false;
                }

                grantedPermissions = await _grantedPermissionProvider.GetPlatformPermissionsAsync(
                    callerContext.CallerUserId,
                    cancellationToken);
                break;
            default:
                return false;
        }

        return HasPermission(grantedPermissions, requiredPermission);
    }

    private static bool HasPermission(IReadOnlySet<string> grantedPermissions, string requiredPermission)
    {
        if (grantedPermissions.Count == 0)
        {
            return false;
        }

        foreach (string grantedPermission in grantedPermissions)
        {
            if (string.IsNullOrWhiteSpace(grantedPermission))
            {
                continue;
            }

            string normalizedGranted = NormalizePermissionCode(grantedPermission);
            if (normalizedGranted == requiredPermission)
            {
                return true;
            }

            if (normalizedGranted.EndsWith(":*", StringComparison.Ordinal))
            {
                string prefix = normalizedGranted[..^1];
                if (requiredPermission.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string NormalizePermissionCode(string permissionCode)
    {
        return permissionCode.Trim().ToUpperInvariant();
    }
}
