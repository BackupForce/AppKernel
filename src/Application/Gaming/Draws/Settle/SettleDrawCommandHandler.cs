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
        if (status != DrawStatus.Drawn)
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
        IReadOnlyCollection<TicketLineResult> existingResults = await ticketLineResultRepository.GetByDrawAndTicketsAsync(
            tenantContext.TenantId,
            draw.Id,
            ticketIds,
            cancellationToken);
        HashSet<(Guid TicketId, int LineIndex)> existingKeys = existingResults
            .Select(result => (result.TicketId, result.LineIndex))
            .ToHashSet();

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            if (!ticketMap.TryGetValue(ticketDraw.TicketId, out Ticket? ticket))
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            if (ticket.SubmissionStatus != TicketSubmissionStatus.Submitted)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            if (ticket.Lines.Count > 1)
            {
                // 目前僅支援單注結算，避免多注資料被誤當成單注處理。
                return Result.Failure(GamingErrors.TicketLinesExceedLimit);
            }

            TicketLine? line = ticket.Lines.FirstOrDefault();
            if (line is null)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            LotteryNumbers? lineNumbers = line.ParseNumbers();
            if (lineNumbers is null)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            PlayTypeCode? playTypeCode = line.PlayTypeCode ?? ticket.PlayTypeCode;
            if (!playTypeCode.HasValue)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            IPlayRule rule = registry.GetRule(draw.GameCode, playTypeCode.Value);
            PrizeTier? tier = rule.Evaluate(lineNumbers, winningNumbers);
            if (tier is not null)
            {
                if (!existingKeys.Contains((ticket.Id, line.LineIndex)))
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
                        option.PayoutAmount,
                        now);

                    ticketLineResultRepository.Insert(result);
                    existingKeys.Add((ticket.Id, line.LineIndex));
                }
            }

            ticketDraw.MarkSettled(now);
            ticketDrawRepository.Update(ticketDraw);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

}
