using Application.Abstractions.Authorization;
using AppPermissionCheckContext = Application.Abstractions.Authorization.PermissionCheckContext;
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
        AppPermissionCheckContext context,
        CallerContext callerContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.PermissionCode))
        {
            return false;
        }

        string requiredPermission = NormalizePermissionCode(context.PermissionCode);
        IReadOnlySet<string> grantedPermissions;

        switch (context.Scope)
        {
            case PermissionScope.Platform:
                grantedPermissions = await _grantedPermissionProvider.GetPlatformPermissionsAsync(
                    callerContext.CallerUserId,
                    cancellationToken);
                break;
            case PermissionScope.Tenant:
                if (!context.TenantId.HasValue)
                {
                    return false;
                }

                grantedPermissions = await _grantedPermissionProvider.GetTenantPermissionsAsync(
                    callerContext.CallerUserId,
                    context.TenantId.Value,
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
