using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Groups.Delete;

internal sealed class DeleteGroupCommandHandler(
    IGroupRepository groupRepository,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteGroupCommand>
{
    public async Task<Result> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        Group? group = await groupRepository.GetByIdAsync(request.Id, cancellationToken);

        if (group is null)
        {
            return Result.Failure(GroupErrors.NotFound);
        }

        groupRepository.Remove(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await invalidator.InvalidateSubjectAsync(SubjectType.Group, group.Id, cancellationToken);
        await invalidator.RemoveGroupIndexAsync(group.Id, cancellationToken);

        return Result.Success();
    }
}
