using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.PrizeRules.Create;

internal sealed class CreatePrizeRuleCommandHandler(
    IPrizeRuleRepository prizeRuleRepository,
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CreatePrizeRuleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePrizeRuleCommand request, CancellationToken cancellationToken)
    {
        Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, request.PrizeId, cancellationToken);
        if (prize is null)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeNotFound);
        }

        bool hasConflict = await prizeRuleRepository.HasActiveRuleAsync(
            tenantContext.TenantId,
            GameType.Lottery539,
            request.MatchCount,
            null,
            cancellationToken);

        if (hasConflict)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeRuleConflict);
        }

        Result<PrizeRule> ruleResult = PrizeRule.Create(
            tenantContext.TenantId,
            GameType.Lottery539,
            request.MatchCount,
            request.PrizeId,
            request.EffectiveFrom,
            request.EffectiveTo,
            dateTimeProvider.UtcNow);

        if (ruleResult.IsFailure)
        {
            return Result.Failure<Guid>(ruleResult.Error);
        }

        PrizeRule rule = ruleResult.Value;
        prizeRuleRepository.Insert(rule);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}
