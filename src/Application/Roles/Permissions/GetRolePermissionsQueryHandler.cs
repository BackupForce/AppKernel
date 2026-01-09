using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class GetRolePermissionsQueryHandler(IRoleRepository roleRepository, IUserContext userContext)
    : IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<string>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure<IReadOnlyList<string>>(RoleErrors.NotFound);
        }

        if (!IsRoleAccessible(userContext, role))
        {
            // 中文註解：跨租戶或 Member 一律拒絕，避免資訊外洩。
            return Result.Failure<IReadOnlyList<string>>(RoleErrors.OperationNotAllowed);
        }

        IReadOnlyList<Permission> permissions = await roleRepository.GetPermissionsByRoleIdAsync(
            request.RoleId,
            cancellationToken);

        List<string> permissionCodes = permissions
            .Select(permission => permission.Name)
            .OrderBy(code => code)
            .ToList();

        return permissionCodes;
    }

    private static bool IsRoleAccessible(IUserContext userContext, Role role)
    {
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
}
