using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Delete;

internal sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.Id, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (!IsRoleAccessible(userContext, role))
        {
            // 中文註解：避免跨租戶或 Member 刪除角色。
            return Result.Failure(RoleErrors.OperationNotAllowed);
        }

        await roleRepository.RemovePermissionsByRoleIdAsync(role.Id, cancellationToken);

        roleRepository.Remove(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.InvalidateRoleAsync(role.Id, cancellationToken);
        await invalidator.RemoveRoleIndexAsync(role.Id, cancellationToken);

        return Result.Success();
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
