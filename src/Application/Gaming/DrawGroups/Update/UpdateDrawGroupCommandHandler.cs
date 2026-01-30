using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.Update;

internal sealed class UpdateDrawGroupCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<UpdateDrawGroupCommand>
{
    public async Task<Result> Handle(UpdateDrawGroupCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.DrawGroupTenantMismatch);
        }

        DrawGroup? drawGroup = await drawGroupRepository.GetByIdAsync(request.TenantId, request.DrawGroupId, cancellationToken);
        if (drawGroup is null)
        {
            return Result.Failure(GamingErrors.DrawGroupNotFound);
        }

        if (drawGroup.Status != DrawGroupStatus.Draft)
        {
            return Result.Failure(GamingErrors.DrawGroupNotDraft);
        }

        Result updateResult = drawGroup.Update(request.Name, request.GrantOpenAtUtc, request.GrantCloseAtUtc);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        drawGroupRepository.Update(drawGroup);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
