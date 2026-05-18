using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Users.AssignRole;

internal sealed class AssignRoleToUserCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignRoleToUserCommand, AssignRoleToUserResultDto>
{
    public async Task<Result<AssignRoleToUserResultDto>> Handle(
        AssignRoleToUserCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AssignRoleToUserResultDto>(UserErrors.NotFound(request.UserId));
        }

        Role? role = await roleRepository.GetByIdAsync(request.RoleId, false, cancellationToken);
        if (role is null)
        {
            return Result.Failure<AssignRoleToUserResultDto>(RoleErrors.NotFound);
        }

        if (!CanAssignRole(user, role))
        {
            // 中文註解：依 UserType 與 TenantId 分流，避免跨租戶/Member 角色污染。
            return Result.Failure<AssignRoleToUserResultDto>(UserErrors.RoleAssignmentNotAllowed);
        }

        if (user.HasRole(role.Id))
        {
            return Result.Failure<AssignRoleToUserResultDto>(UserErrors.RoleAlreadyAssigned(role.Id));
        }

        user.AssignRole(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.TrackRoleUserAsync(role.Id, user.Id, cancellationToken);
        await invalidator.InvalidateUserAsync(user.Id, cancellationToken);

        List<int> roleIds = user.Roles.Select(r => r.Id).OrderBy(id => id).ToList();
        AssignRoleToUserResultDto response = new AssignRoleToUserResultDto(user.Id, roleIds);

        return Result.Success(response);
    }

    private static bool CanAssignRole(User user, Role role)
    {
        if (user.Type == UserType.Platform)
        {
            return role.IsPlatformRole();
        }

        if (user.Type == UserType.Tenant)
        {
            return user.TenantId.HasValue
                && role.TenantId.HasValue
                && role.TenantId.Value == user.TenantId.Value;
        }

        // 中文註解：Member 不允許指派角色，Fail Closed。
        return false;
    }
}
