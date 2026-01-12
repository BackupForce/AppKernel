using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.PrizeRules.Update;

internal sealed class UpdatePrizeRuleCommandHandler(
    IPrizeRuleRepository prizeRuleRepository,
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdatePrizeRuleCommand>
{
    public async Task<Result> Handle(UpdatePrizeRuleCommand request, CancellationToken cancellationToken)
    {
        PrizeRule? rule = await prizeRuleRepository.GetByIdAsync(tenantContext.TenantId, request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(GamingErrors.PrizeRuleNotFound);
        }

        Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, request.PrizeId, cancellationToken);
        if (prize is null)
        {
            return Result.Failure(GamingErrors.PrizeNotFound);
        }

        bool hasConflict = await prizeRuleRepository.HasActiveRuleAsync(
            tenantContext.TenantId,
            GameType.Lottery539,
            request.MatchCount,
            request.RuleId,
            cancellationToken);

        if (hasConflict)
        {
            return Result.Failure(GamingErrors.PrizeRuleConflict);
        }

        rule.Update(request.MatchCount, request.PrizeId, request.EffectiveFrom, request.EffectiveTo, dateTimeProvider.UtcNow);
        prizeRuleRepository.Update(rule);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
