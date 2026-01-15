using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Draws;
using Domain.Gaming.PrizeAwards;
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
    IPrizeAwardRepository prizeAwardRepository,
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

        if (draw.Status != DrawStatus.Settled)
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

        DateTime now = dateTimeProvider.UtcNow;

        // 結算時依期數批次取得所有票券。
        IReadOnlyCollection<Ticket> tickets = await ticketRepository.GetByDrawIdAsync(
            tenantContext.TenantId,
            draw.Id,
            cancellationToken);

        foreach (Ticket ticket in tickets)
        {
            foreach (TicketLine line in ticket.Lines)
            {
                LotteryNumbers? lineNumbers = line.ParseNumbers();
                if (lineNumbers is null)
                {
                    continue;
                }

                IPlayRule rule = registry.GetRule(draw.GameCode, ticket.PlayTypeCode);
                PrizeTier? tier = rule.Evaluate(lineNumbers, winningNumbers);
                if (tier is null)
                {
                    continue;
                }

                int matchedCount = Lottery539MatchCalculator.CalculateMatchedCount(
                    winningNumbers.Numbers,
                    lineNumbers.Numbers);

                // 防重：以 TenantId + DrawId + TicketId + LineIndex 作為唯一鍵概念。
                bool exists = await prizeAwardRepository.ExistsAsync(
                    tenantContext.TenantId,
                    draw.Id,
                    ticket.Id,
                    line.LineIndex,
                    cancellationToken);

                if (exists)
                {
                    // 結算重跑時必須具備 idempotency，避免重複產生 Award。
                    continue;
                }

                PrizeOption? option = draw.FindPrizeOption(ticket.PlayTypeCode, tier.Value);
                if (option is null)
                {
                    return Result.Failure(GamingErrors.PrizePoolIncomplete);
                }

                PrizeAward award = PrizeAward.Create(
                    tenantContext.TenantId,
                    ticket.MemberId,
                    draw.Id,
                    draw.GameCode,
                    ticket.PlayTypeCode,
                    ticket.Id,
                    line.LineIndex,
                    matchedCount,
                    tier.Value,
                    option,
                    ResolveAwardExpiry(option, draw, now),
                    now);

                prizeAwardRepository.Insert(award);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static DateTime? ResolveAwardExpiry(PrizeOption option, Draw draw, DateTime now)
    {
        int? validDays = option.RedeemValidDays ?? draw.RedeemValidDays;
        if (!validDays.HasValue || validDays.Value <= 0)
        {
            return null;
        }

        return now.AddDays(validDays.Value);
    }
}
