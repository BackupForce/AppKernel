using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using Domain.Gaming.Services;
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
    IPrizeRuleRepository prizeRuleRepository,
    IPrizeAwardRepository prizeAwardRepository,
    IPrizeAwardOptionRepository prizeAwardOptionRepository,
    IDrawPrizeMappingRepository drawPrizeMappingRepository,
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<SettleDrawCommand>
{
    public async Task<Result> Handle(SettleDrawCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        if (draw.Status != DrawStatus.Settled)
        {
            return Result.Failure(GamingErrors.DrawNotSettled);
        }

        LotteryNumbers? winningNumbers = draw.GetWinningNumbers();
        if (winningNumbers is null)
        {
            return Result.Failure(GamingErrors.DrawNotSettled);
        }

        DateTime now = dateTimeProvider.UtcNow;
        // 取得結算時點的有效規則，避免未來規則變動影響歷史期數。
        IReadOnlyCollection<PrizeRule> rules = await prizeRuleRepository.GetActiveRulesAsync(
            tenantContext.TenantId,
            GameType.Lottery539,
            now,
            cancellationToken);

        IReadOnlyCollection<DrawPrizeMapping> mappings = await drawPrizeMappingRepository.GetByDrawIdAsync(
            tenantContext.TenantId,
            draw.Id,
            cancellationToken);

        Dictionary<int, List<Guid>> mappingByMatch = mappings
            .GroupBy(mapping => mapping.MatchCount)
            .ToDictionary(group => group.Key, group => group.Select(item => item.PrizeId).ToList());

        // 結算時依期數批次取得所有票券。
        IReadOnlyCollection<Ticket> tickets = await ticketRepository.GetByDrawIdAsync(
            tenantContext.TenantId,
            draw.Id,
            cancellationToken);

        foreach (Ticket ticket in tickets)
        {
            foreach (TicketLine line in ticket.Lines)
            {
                LotteryNumbers? lineNumbers = line.GetNumbers();
                if (lineNumbers is null)
                {
                    continue;
                }

                int matchedCount = Lottery539MatchCalculator.CalculateMatchedCount(
                    winningNumbers.Numbers,
                    lineNumbers.Numbers);

                // 將命中數轉換為獎品規則，若無匹配則不產生 Award。
                PrizeRule? rule = PrizeRuleResolver.Resolve(rules, matchedCount, now);
                if (rule is null)
                {
                    continue;
                }

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

                PrizeAward award = PrizeAward.Create(
                    tenantContext.TenantId,
                    ticket.MemberId,
                    draw.Id,
                    ticket.Id,
                    line.LineIndex,
                    matchedCount,
                    rule.PrizeId,
                    ResolveAwardExpiry(rule, draw, now),
                    now);

                prizeAwardRepository.Insert(award);

                // 中文註解：AwardOptions 使用期數設定快照，避免後台改動影響已結算紀錄。
                IReadOnlyCollection<Guid> optionPrizeIds = ResolveOptionPrizeIds(mappingByMatch, matchedCount, rule.PrizeId);
                List<PrizeAwardOption> options = new List<PrizeAwardOption>();
                foreach (Guid prizeId in optionPrizeIds)
                {
                    Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, prizeId, cancellationToken);
                    if (prize is null)
                    {
                        continue;
                    }

                    PrizeAwardOption option = PrizeAwardOption.Create(
                        tenantContext.TenantId,
                        award.Id,
                        prize.Id,
                        prize.Name,
                        prize.Cost,
                        now);
                    options.Add(option);
                }

                prizeAwardOptionRepository.InsertRange(options);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static DateTime? ResolveAwardExpiry(PrizeRule rule, Draw draw, DateTime now)
    {
        int? validDays = rule.RedeemValidDays ?? draw.RedeemValidDays;
        if (!validDays.HasValue || validDays.Value <= 0)
        {
            return null;
        }

        return now.AddDays(validDays.Value);
    }

    private static IReadOnlyCollection<Guid> ResolveOptionPrizeIds(
        IReadOnlyDictionary<int, List<Guid>> mappingByMatch,
        int matchedCount,
        Guid fallbackPrizeId)
    {
        if (mappingByMatch.TryGetValue(matchedCount, out List<Guid>? prizes) && prizes.Count > 0)
        {
            return prizes;
        }

        // 中文註解：若期數未配置該命中數的兌獎清單，退回規則指定的獎品避免無法兌獎。
        return new List<Guid> { fallbackPrizeId };
    }
}
