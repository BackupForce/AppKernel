using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using Domain.Gaming.Services;
using SharedKernel;

namespace Application.Gaming.Draws.Settle;

internal sealed class SettleDrawCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    IPrizeRuleRepository prizeRuleRepository,
    IPrizeAwardRepository prizeAwardRepository,
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
        IReadOnlyCollection<PrizeRule> rules = await prizeRuleRepository.GetActiveRulesAsync(
            tenantContext.TenantId,
            GameType.Lottery539,
            now,
            cancellationToken);

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

                PrizeRule? rule = PrizeRuleResolver.Resolve(rules, matchedCount, now);
                if (rule is null)
                {
                    continue;
                }

                bool exists = await prizeAwardRepository.ExistsAsync(
                    tenantContext.TenantId,
                    draw.Id,
                    ticket.Id,
                    line.LineIndex,
                    cancellationToken);

                if (exists)
                {
                    // 中文註解：結算重跑時必須具備 idempotency，避免重複產生 Award。
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
                    now);

                prizeAwardRepository.Insert(award);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
