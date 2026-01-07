using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Users.RemoveGroup;

internal sealed class RemoveGroupFromUserCommandHandler(
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveGroupFromUserCommand, RemoveGroupFromUserResultDto>
{
    public async Task<Result<RemoveGroupFromUserResultDto>> Handle(
        RemoveGroupFromUserCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdWithGroupsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<RemoveGroupFromUserResultDto>(UserErrors.NotFound(request.UserId));
        }

        Group? group = await groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
        {
            return Result.Failure<RemoveGroupFromUserResultDto>(GroupErrors.NotFound);
        }

        if (!user.HasGroup(group.Id))
        {
            return Result.Failure<RemoveGroupFromUserResultDto>(UserErrors.GroupNotAssigned(group.Id));
        }

        user.RemoveGroup(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await invalidator.InvalidateSubjectAsync(SubjectType.Group, group.Id, cancellationToken);
        await invalidator.InvalidateUserAsync(user.Id, cancellationToken);

        var groupIds = user.UserGroups.Select(userGroup => userGroup.GroupId).OrderBy(id => id).ToList();
        var response = new RemoveGroupFromUserResultDto(user.Id, groupIds);

        return Result.Success(response);
    }
}
