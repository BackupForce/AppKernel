using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.PrizeRules.Deactivate;

internal sealed class DeactivatePrizeRuleCommandHandler(
    IPrizeRuleRepository prizeRuleRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<DeactivatePrizeRuleCommand>
{
    public async Task<Result> Handle(DeactivatePrizeRuleCommand request, CancellationToken cancellationToken)
    {
        PrizeRule? rule = await prizeRuleRepository.GetByIdAsync(tenantContext.TenantId, request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(GamingErrors.PrizeRuleNotFound);
        }

        rule.Deactivate(dateTimeProvider.UtcNow);
        prizeRuleRepository.Update(rule);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
