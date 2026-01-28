using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Draws.Settle;

/// <summary>
/// 結算開獎結果，依命中數產生得獎記錄。
/// </summary>
/// <remarks>
/// Application 層負責協調規則與 Repository，避免 Domain 依賴外部資料來源。
/// </remarks>
internal sealed class SettleDrawCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    ITicketLineResultRepository ticketLineResultRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<SettleDrawCommand>
{
    public async Task<Result> Handle(SettleDrawCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure(entitlementResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        DrawStatus status = draw.GetEffectiveStatus(now);
        if (status != DrawStatus.Settled)
        {
            return Result.Failure(GamingErrors.DrawNotSettled);
        }

        LotteryNumbers? winningNumbers = draw.ParseWinningNumbers();
        if (winningNumbers is null)
        {
            return Result.Failure(GamingErrors.DrawNotSettled);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        Result prizePoolResult = draw.EnsurePrizePoolCompleteForSettlement(registry);
        if (prizePoolResult.IsFailure)
        {
            return Result.Failure(prizePoolResult.Error);
        }

        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByDrawIdAsync(
            tenantContext.TenantId,
            draw.Id,
            TicketDrawParticipationStatus.Active,
            cancellationToken);

        if (ticketDraws.Count == 0)
        {
            return Result.Success();
        }

        IReadOnlyCollection<Guid> ticketIds = ticketDraws.Select(item => item.TicketId).Distinct().ToList();
        IReadOnlyCollection<Ticket> tickets = await ticketRepository.GetByIdsAsync(
            tenantContext.TenantId,
            ticketIds,
            cancellationToken);
        Dictionary<Guid, Ticket> ticketMap = tickets.ToDictionary(ticket => ticket.Id, ticket => ticket);

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            if (!ticketMap.TryGetValue(ticketDraw.TicketId, out Ticket? ticket))
            {
                continue;
            }

            TicketLine? line = ticket.Lines.FirstOrDefault(item => item.LineIndex == 0);
            if (line is null)
            {
                ticketDraw.MarkSettled(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            LotteryNumbers? lineNumbers = line.ParseNumbers();
            if (lineNumbers is null)
            {
                ticketDraw.MarkSettled(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            PlayTypeCode? playTypeCode = line.PlayTypeCode ?? ticket.PlayTypeCode;
            if (!playTypeCode.HasValue)
            {
                return Result.Failure(GamingErrors.PlayTypeCodeRequired);
            }

            IPlayRule rule = registry.GetRule(draw.GameCode, playTypeCode.Value);
            PrizeTier? tier = rule.Evaluate(lineNumbers, winningNumbers);
            if (tier is not null)
            {
                bool exists = await ticketLineResultRepository.ExistsAsync(
                    tenantContext.TenantId,
                    ticket.Id,
                    draw.Id,
                    line.LineIndex,
                    cancellationToken);

                if (!exists)
                {
                    PrizeOption? option = draw.FindPrizeOption(playTypeCode.Value, tier.Value);
                    if (option is null)
                    {
                        return Result.Failure(GamingErrors.PrizePoolNotConfigured);
                    }

                    TicketLineResult result = TicketLineResult.Create(
                        tenantContext.TenantId,
                        ticket.Id,
                        draw.Id,
                        line.LineIndex,
                        tier.Value,
                        option.Cost,
                        now);

                    ticketLineResultRepository.Insert(result);
                }
            }

            ticketDraw.MarkSettled(now);
            ticketDrawRepository.Update(ticketDraw);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

}
