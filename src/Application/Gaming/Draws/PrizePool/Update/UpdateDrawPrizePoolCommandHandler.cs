using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Draws.PrizePool.Update;

internal sealed class UpdateDrawPrizePoolCommandHandler(
    IDrawRepository drawRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<UpdateDrawPrizePoolCommand, DrawPrizePoolDto>
{
    public async Task<Result<DrawPrizePoolDto>> Handle(UpdateDrawPrizePoolCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<DrawPrizePoolDto>(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<DrawPrizePoolDto>(entitlementResult.Error);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();

        foreach (UpdateDrawPrizePoolItem item in request.Items)
        {
            Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(item.PlayTypeCode);
            if (playTypeResult.IsFailure)
            {
                return Result.Failure<DrawPrizePoolDto>(playTypeResult.Error);
            }

            Result<PrizeTier> tierResult = PrizeTier.Create(item.Tier);
            if (tierResult.IsFailure)
            {
                return Result.Failure<DrawPrizePoolDto>(tierResult.Error);
            }

            Result<PrizeOption> optionResult = PrizeOption.Create(
                item.Option.Name,
                item.Option.Cost,
                item.Option.PayoutAmount,
                item.Option.RedeemValidDays,
                item.Option.Description,
                item.Option.PrizeId);
            if (optionResult.IsFailure)
            {
                return Result.Failure<DrawPrizePoolDto>(optionResult.Error);
            }

            Result configureResult = draw.ConfigurePrizeOption(
                playTypeResult.Value,
                tierResult.Value,
                optionResult.Value,
                registry);
            if (configureResult.IsFailure)
            {
                return Result.Failure<DrawPrizePoolDto>(configureResult.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return DrawPrizePoolMapper.ToDto(draw);
    }
}
