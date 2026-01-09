using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class RemoveRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveRolePermissionsCommand>
{
    public async Task<Result> Handle(RemoveRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (!IsRoleAccessible(userContext, role))
        {
            // 中文註解：避免跨租戶或 Member 操作角色權限。
            return Result.Failure(RoleErrors.OperationNotAllowed);
        }

        HashSet<string> codes = new HashSet<string>(StringComparer.Ordinal);
        foreach (string code in request.PermissionCodes)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                codes.Add(code.Trim().ToUpperInvariant());
            }
        }

        if (codes.Count == 0)
        {
            return Result.Failure(RoleErrors.PermissionCodesRequired);
        }

        await roleRepository.RemovePermissionsAsync(request.RoleId, codes, cancellationToken);
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
}
