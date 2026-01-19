using Application.Abstractions.Authentication;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Draws.PrizePool.Validate;

internal sealed class ValidateDrawPrizePoolQueryHandler(
    IDrawRepository drawRepository,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : IQueryHandler<ValidateDrawPrizePoolQuery, DrawPrizePoolValidationDto>
{
    public async Task<Result<DrawPrizePoolValidationDto>> Handle(ValidateDrawPrizePoolQuery request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<DrawPrizePoolValidationDto>(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<DrawPrizePoolValidationDto>(entitlementResult.Error);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        List<MissingPrizePoolSlotDto> missing = new List<MissingPrizePoolSlotDto>();

        foreach (PlayTypeCode playType in draw.EnabledPlayTypes)
        {
            IPlayRule rule = registry.GetRule(draw.GameCode, playType);
            foreach (PrizeTier tier in rule.GetTiers())
            {
                DrawPrizePoolItem? slot = draw.PrizePool.FirstOrDefault(item => item.PlayTypeCode == playType && item.Tier == tier);
                if (slot is null || slot.Option is null)
                {
                    missing.Add(new MissingPrizePoolSlotDto(playType.Value, tier.Value));
                }
            }
        }

        return new DrawPrizePoolValidationDto(missing.Count == 0, missing);
    }
}
