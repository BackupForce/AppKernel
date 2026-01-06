using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

// 基於 JWT permissions claim 的授權處理器，直接解析權限字串，不查詢資料庫
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is null || context.User.Identity is null)
        {
            return Task.CompletedTask;
        }

        if (!context.User.Identity.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        Claim? permissionsClaim = context.User.FindFirst("permissions");
        if (permissionsClaim is null || string.IsNullOrWhiteSpace(permissionsClaim.Value))
        {
            // 沒有權限清單直接拒絕
            return Task.CompletedTask;
        }

        // 解析 JWT 的 permissions claim
        string[] userPermissions = permissionsClaim.Value.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        string requiredPermission = requirement.PermissionCode;

        if (UserHasExactPermission(userPermissions, requiredPermission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 支援 "xxx:*" 的 wildcard 權限
        if (UserHasWildcardPermission(userPermissions, requiredPermission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool UserHasExactPermission(string[] userPermissions, string requiredPermission)
    {
        for (int index = 0; index < userPermissions.Length; index++)
        {
            if (string.Equals(userPermissions[index], requiredPermission, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool UserHasWildcardPermission(string[] userPermissions, string requiredPermission)
    {
        int separatorIndex = requiredPermission.IndexOf(':');
        if (separatorIndex <= 0)
        {
            return false;
        }

        string requiredPrefix = requiredPermission.Substring(0, separatorIndex);
        string wildcardPermission = requiredPrefix + ":*";

        for (int index = 0; index < userPermissions.Length; index++)
        {
            if (string.Equals(userPermissions[index], wildcardPermission, StringComparison.OrdinalIgnoreCase) && requiredPermission.StartsWith(requiredPrefix + ":", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
