using Application.Abstractions.Authentication;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Draws.PrizePool.Get;

internal sealed class GetDrawPrizePoolQueryHandler(
    IDrawRepository drawRepository,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetDrawPrizePoolQuery, DrawPrizePoolDto>
{
    public async Task<Result<DrawPrizePoolDto>> Handle(GetDrawPrizePoolQuery request, CancellationToken cancellationToken)
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

        return DrawPrizePoolMapper.ToDto(draw);
    }
}
