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

        if (drawGroup.Status != DrawGroupStatus.Disabled)
        {
            return Result.Failure(GamingErrors.DrawGroupNotDisabled);
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

        IReadOnlyCollection<DrawGrantWindow> grantWindows = await BuildGrantWindowsAsync(
            drawGroup,
            draw,
            cancellationToken);

        Result addResult = drawGroup.AddDraw(request.DrawId, dateTimeProvider.UtcNow, grantWindows);
        if (addResult.IsFailure)
        {
            return addResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<IReadOnlyCollection<DrawGrantWindow>> BuildGrantWindowsAsync(
        DrawGroup drawGroup,
        Draw addedDraw,
        CancellationToken cancellationToken)
    {
        List<Guid> existingDrawIds = drawGroup.Draws
            .Select(item => item.DrawId)
            .ToList();

        List<DrawGrantWindow> windows = new();

        if (existingDrawIds.Count > 0)
        {
            IReadOnlyCollection<Draw> existingDraws = await drawRepository.GetByIdsAsync(
                drawGroup.TenantId,
                existingDrawIds,
                cancellationToken);

            windows.AddRange(existingDraws.Select(ToGrantWindow));
        }

        if (!existingDrawIds.Contains(addedDraw.Id))
        {
            windows.Add(ToGrantWindow(addedDraw));
        }

        return windows;
    }

    private static DrawGrantWindow ToGrantWindow(Draw draw)
    {
        DateTime closeAtUtc = draw.ManualCloseAt ?? draw.SalesCloseAt;
        return new DrawGrantWindow(draw.SalesOpenAt, closeAtUtc);
    }
}
