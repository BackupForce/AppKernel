using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.Draws.Remove;

internal sealed class RemoveDrawGroupDrawCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<RemoveDrawGroupDrawCommand>
{
    public async Task<Result> Handle(RemoveDrawGroupDrawCommand request, CancellationToken cancellationToken)
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

        Result removeResult = drawGroup.RemoveDraw(request.DrawId);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
