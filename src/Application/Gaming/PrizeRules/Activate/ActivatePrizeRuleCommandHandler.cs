using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.PrizeRules.Activate;

internal sealed class ActivatePrizeRuleCommandHandler(
    IPrizeRuleRepository prizeRuleRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivatePrizeRuleCommand>
{
    public async Task<Result> Handle(ActivatePrizeRuleCommand request, CancellationToken cancellationToken)
    {
        PrizeRule? rule = await prizeRuleRepository.GetByIdAsync(tenantContext.TenantId, request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(GamingErrors.PrizeRuleNotFound);
        }

        bool hasConflict = await prizeRuleRepository.HasActiveRuleAsync(
            tenantContext.TenantId,
            rule.GameType,
            rule.MatchCount,
            rule.Id,
            cancellationToken);

        if (hasConflict)
        {
            return Result.Failure(GamingErrors.PrizeRuleConflict);
        }

        rule.Activate(dateTimeProvider.UtcNow);
        prizeRuleRepository.Update(rule);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
