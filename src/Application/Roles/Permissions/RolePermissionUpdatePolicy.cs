using Application.Abstractions.Authentication;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Permissions;

internal static class RolePermissionUpdatePolicy
{
    public static bool IsRoleAccessible(IUserContext userContext, Role role)
    {
        if (userContext.UserType == UserType.Member)
        {
            return false;
        }

        if (userContext.UserType == UserType.Platform)
        {
            return role.IsPlatformRole();
        }

        if (userContext.UserType == UserType.Tenant)
        {
            return userContext.TenantId.HasValue
                && role.TenantId == userContext.TenantId.Value;
        }

        return false;
    }

    public static HashSet<string> NormalizeCodes(IEnumerable<string>? permissionCodes)
    {
        HashSet<string> normalized = new(StringComparer.Ordinal);

        if (permissionCodes is null)
        {
            return normalized;
        }

        foreach (string code in permissionCodes)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                normalized.Add(PermissionCatalog.NormalizeCode(code));
            }
        }

        return normalized;
    }

    public static Result ValidateRequestedCodes(Role role, IReadOnlySet<string> requestedCodes, bool requireNonEmpty)
    {
        if (requireNonEmpty && requestedCodes.Count == 0)
        {
            return Result.Failure(RoleErrors.PermissionCodesRequired);
        }

        PermissionScope expectedScope = role.IsPlatformRole()
            ? PermissionScope.Platform
            : PermissionScope.Tenant;

        foreach (string code in requestedCodes)
        {
            if (!PermissionCatalog.TryGetScope(code, out PermissionScope scope))
            {
                return Result.Failure(RoleErrors.InvalidPermissionCode(code));
            }

            if (scope != expectedScope)
            {
                return Result.Failure(RoleErrors.PermissionScopeMismatch);
            }
        }

        return Result.Success();
    }
}
