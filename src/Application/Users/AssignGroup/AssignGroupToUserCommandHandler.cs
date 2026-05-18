using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Users.AssignGroup;

internal sealed class AssignGroupToUserCommandHandler(
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignGroupToUserCommand, AssignGroupToUserResultDto>
{
    public async Task<Result<AssignGroupToUserResultDto>> Handle(
        AssignGroupToUserCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdWithGroupsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AssignGroupToUserResultDto>(UserErrors.NotFound(request.UserId));
        }

        Group? group = await groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
        {
            return Result.Failure<AssignGroupToUserResultDto>(GroupErrors.NotFound);
        }

        if (user.HasGroup(group.Id))
        {
            return Result.Failure<AssignGroupToUserResultDto>(UserErrors.GroupAlreadyAssigned(group.Id));
        }

        user.AssignGroup(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await invalidator.InvalidateSubjectAsync(SubjectType.Group, group.Id, cancellationToken);
        await invalidator.TrackGroupUserAsync(group.Id, user.Id, cancellationToken);
        await invalidator.InvalidateUserAsync(user.Id, cancellationToken);

        var groupIds = user.UserGroups.Select(userGroup => userGroup.GroupId).OrderBy(id => id).ToList();
        var response = new AssignGroupToUserResultDto(user.Id, groupIds);

        return Result.Success(response);
    }
}
