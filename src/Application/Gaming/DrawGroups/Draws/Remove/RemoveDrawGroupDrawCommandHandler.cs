using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.Draws.Remove;

internal sealed class RemoveDrawGroupDrawCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IDrawRepository drawRepository,
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

        if (drawGroup.Status != DrawGroupStatus.Disabled)
        {
            return Result.Failure(GamingErrors.DrawGroupNotDisabled);
        }

        IReadOnlyCollection<DrawGrantWindow> grantWindows = await BuildGrantWindowsAsync(
            drawGroup,
            request.DrawId,
            cancellationToken);

        Result removeResult = drawGroup.RemoveDraw(request.DrawId, grantWindows);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<IReadOnlyCollection<DrawGrantWindow>> BuildGrantWindowsAsync(
        DrawGroup drawGroup,
        Guid removedDrawId,
        CancellationToken cancellationToken)
    {
        List<Guid> remainingDrawIds = drawGroup.Draws
            .Select(item => item.DrawId)
            .Where(drawId => drawId != removedDrawId)
            .ToList();

        if (remainingDrawIds.Count == 0)
        {
            return Array.Empty<DrawGrantWindow>();
        }

        IReadOnlyCollection<Draw> remainingDraws = await drawRepository.GetByIdsAsync(
            drawGroup.TenantId,
            remainingDrawIds,
            cancellationToken);

        return remainingDraws
            .Select(ToGrantWindow)
            .ToList();
    }

    private static DrawGrantWindow ToGrantWindow(Draw draw)
    {
        DateTime closeAtUtc = draw.ManualCloseAt ?? draw.SalesCloseAt;
        return new DrawGrantWindow(draw.SalesOpenAt, closeAtUtc);
    }
}
