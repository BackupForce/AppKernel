using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Draws.Reopen;

/// <summary>
/// 解封期數處理器。
/// </summary>
internal sealed class ReopenDrawCommandHandler(
    IDrawRepository drawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<ReopenDrawCommand>
{
    public async Task<Result> Handle(ReopenDrawCommand request, CancellationToken cancellationToken)
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

        DateTime now = dateTimeProvider.UtcNow;
        DrawStatus status = draw.GetEffectiveStatus(now);
        if (status == DrawStatus.Drawn || !string.IsNullOrWhiteSpace(draw.WinningNumbersRaw))
        {
            return Result.Failure(GamingErrors.DrawAlreadyExecuted);
        }

        if (!draw.IsWithinSalesTimeRange(now))
        {
            // 中文註解：解封仍需在可下注時間內，避免解封後仍無法下注造成混亂。
            return Result.Failure(GamingErrors.DrawReopenWindowInvalid);
        }

        draw.Reopen(now);
        drawRepository.Update(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
