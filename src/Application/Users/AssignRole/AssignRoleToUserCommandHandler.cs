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

        if (user.HasRole(role.Id))
        {
            return Result.Failure<AssignRoleToUserResultDto>(UserErrors.RoleAlreadyAssigned(role.Id));
        }

        user.AssignRole(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.TrackRoleUserAsync(role.Id, user.Id, cancellationToken);
        await invalidator.InvalidateUserAsync(user.Id, cancellationToken);

        var roleIds = user.Roles.Select(r => r.Id).OrderBy(id => id).ToList();
        var response = new AssignRoleToUserResultDto(user.Id, roleIds);

        return Result.Success(response);
    }
}
