using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Draws.ManualClose;

/// <summary>
/// 手動封盤處理器。
/// </summary>
internal sealed class CloseDrawManuallyCommandHandler(
    IDrawRepository drawRepository,
    ITicketDrawRepository ticketDrawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<CloseDrawManuallyCommand>
{
    public async Task<Result> Handle(CloseDrawManuallyCommand request, CancellationToken cancellationToken)
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

        if (draw.Status == DrawStatus.Settled || !string.IsNullOrWhiteSpace(draw.WinningNumbersRaw))
        {
            return Result.Failure(GamingErrors.DrawAlreadyExecuted);
        }

        DateTime now = dateTimeProvider.UtcNow;
        draw.CloseManually(request.Reason, now);
        drawRepository.Update(draw);

        if (draw.IsEffectivelyClosed(now))
        {
            IReadOnlyCollection<TicketDraw> pendingTicketDraws = await ticketDrawRepository.GetPendingForUnsubmittedTicketsAsync(
                tenantContext.TenantId,
                draw.Id,
                cancellationToken);

            foreach (TicketDraw ticketDraw in pendingTicketDraws)
            {
                ticketDraw.MarkInvalid(now);
            }

            ticketDrawRepository.UpdateRange(pendingTicketDraws);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
