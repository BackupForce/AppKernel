using Application.Gaming.Tickets.Services;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Redeem;

internal sealed class TicketRedeemableDrawService(
    IDrawRepository drawRepository,
    IDrawGroupDrawRepository drawGroupDrawRepository)
{
    public async Task<Result<IReadOnlyCollection<Guid>>> GetRedeemableDrawIdsAsync(
        Guid tenantId,
        Ticket ticket,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        _ = nowUtc;
        if (!ticket.DrawId.HasValue)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(GamingErrors.DrawNotFound);
        }

        if (ticket.DrawGroupId.HasValue)
        {
            IReadOnlyCollection<DrawGroupDraw> drawGroupDraws = await drawGroupDrawRepository.GetByDrawGroupIdAsync(
                tenantId,
                ticket.DrawGroupId.Value,
                cancellationToken);

            List<Guid> drawIds = drawGroupDraws.Select(item => item.DrawId).Distinct().ToList();
            IReadOnlyCollection<Draw> draws = drawIds.Count == 0
                ? Array.Empty<Draw>()
                : await drawRepository.GetByIdsAsync(tenantId, drawIds, cancellationToken);

            Draw? primary = draws.FirstOrDefault(draw => draw.Id == ticket.DrawId.Value);
            if (primary is null)
            {
                return Result.Failure<IReadOnlyCollection<Guid>>(GamingErrors.DrawNotFound);
            }

            DateTime primaryDrawAt = primary.DrawAt;
            IReadOnlyCollection<Guid> redeemable = draws
                .Where(draw => draw.DrawAt >= primaryDrawAt)
                .Where(DrawEligibility.IsRedeemable)
                .OrderBy(draw => draw.DrawAt)
                .Select(draw => draw.Id)
                .ToList();

            return Result.Success(redeemable);
        }

        Draw? single = await drawRepository.GetByIdAsync(tenantId, ticket.DrawId.Value, cancellationToken);
        if (single is null)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(GamingErrors.DrawNotFound);
        }

        if (!DrawEligibility.IsRedeemable(single))
        {
            return Result.Success<IReadOnlyCollection<Guid>>(Array.Empty<Guid>());
        }

        return Result.Success<IReadOnlyCollection<Guid>>(new[] { single.Id });
    }
}
