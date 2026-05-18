using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Update;

internal sealed class UpdateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(RoleErrors.NameRequired);
        }

        Role? role = await roleRepository.GetByIdAsync(request.Id, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (!IsRoleAccessible(userContext, role))
        {
            // 中文註解：避免跨租戶或 Member 角色修改，Fail Closed。
            return Result.Failure(RoleErrors.OperationNotAllowed);
        }

        string name = request.Name.Trim();

        bool isUnique = await roleRepository.IsNameUniqueAsync(name, role.TenantId, request.Id, cancellationToken);
        if (!isUnique)
        {
            return Result.Failure(RoleErrors.NameConflict);
        }

        role.Rename(name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

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

        // 中文註解：未知狀態一律拒絕。
        return false;
    }
}
