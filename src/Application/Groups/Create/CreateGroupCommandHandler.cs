using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Groups.Create;

internal sealed class CreateGroupCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateGroupCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<Guid>(GroupErrors.NameRequired);
        }

        string name = request.Name.Trim();
        string externalKey = request.ExternalKey?.Trim() ?? string.Empty;

        bool isUnique = await groupRepository.IsNameUniqueAsync(name, null, cancellationToken);
        if (!isUnique)
        {
            return Result.Failure<Guid>(GroupErrors.NameConflict);
        }

        Group group = Group.Create(name, externalKey);

        groupRepository.Insert(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return group.Id;
    }
}
