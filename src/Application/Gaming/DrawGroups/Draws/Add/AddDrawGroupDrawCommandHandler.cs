using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.Draws.Add;

internal sealed class AddDrawGroupDrawCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IDrawRepository drawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<AddDrawGroupDrawCommand>
{
    public async Task<Result> Handle(AddDrawGroupDrawCommand request, CancellationToken cancellationToken)
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

        Draw? draw = await drawRepository.GetByIdAsync(request.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        if (draw.GameCode != drawGroup.GameCode)
        {
            return Result.Failure(GamingErrors.DrawGroupDrawGameCodeMismatch);
        }

        Result addResult = drawGroup.AddDraw(request.DrawId, dateTimeProvider.UtcNow);
        if (addResult.IsFailure)
        {
            return addResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
