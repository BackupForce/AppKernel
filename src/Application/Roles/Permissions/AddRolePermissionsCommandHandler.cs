using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class AddRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddRolePermissionsCommand>
{
    public async Task<Result> Handle(AddRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, true, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (!IsRoleAccessible(userContext, role))
        {
            // 中文註解：避免跨租戶或 Member 操作角色權限。
            return Result.Failure(RoleErrors.OperationNotAllowed);
        }

        HashSet<string> existingCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (Permission permission in role.Permissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            existingCodes.Add(permission.Name.Trim().ToUpperInvariant());
        }

        HashSet<string> requestedCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (string code in request.PermissionCodes)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                requestedCodes.Add(code.Trim().ToUpperInvariant());
            }
        }

        if (requestedCodes.Count == 0)
        {
            return Result.Failure(RoleErrors.PermissionCodesRequired);
        }

        if (!ArePermissionScopesAllowedForRole(role, requestedCodes))
        {
            // 中文註解：平台/租戶角色不可混用權限，Fail Closed。
            return Result.Failure(RoleErrors.PermissionScopeMismatch);
        }

        List<Permission> permissionsToAdd = new List<Permission>();
        foreach (string code in requestedCodes)
        {
            if (!existingCodes.Contains(code))
            {
                Permission permission = Permission.CreateForRole(code, string.Empty, role.Id);
                permissionsToAdd.Add(permission);
            }
        }

        if (permissionsToAdd.Count == 0)
        {
            return Result.Success();
        }

        await roleRepository.AddPermissionsAsync(permissionsToAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.InvalidateRoleAsync(request.RoleId, cancellationToken);

        return Result.Success();
    }

    private static bool IsRoleAccessible(IUserContext userContext, Role role)
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

    private static bool ArePermissionScopesAllowedForRole(Role role, IEnumerable<string> requestedCodes)
    {
        PermissionScope expectedScope = role.IsPlatformRole()
            ? PermissionScope.Platform
            : PermissionScope.Tenant;

        foreach (string code in requestedCodes)
        {
            if (!PermissionCatalog.TryGetScope(code, out PermissionScope scope))
            {
                // 中文註解：無法解析的權限碼一律拒絕。
                return false;
            }

            if (scope != expectedScope)
            {
                return false;
            }
        }

        return true;
    }
}
