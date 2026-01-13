using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.Draws.ManualClose;

/// <summary>
/// 手動封盤處理器。
/// </summary>
internal sealed class CloseDrawManuallyCommandHandler(
    IDrawRepository drawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CloseDrawManuallyCommand>
{
    public async Task<Result> Handle(CloseDrawManuallyCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        if (draw.Status == DrawStatus.Settled || !string.IsNullOrWhiteSpace(draw.WinningNumbersRaw))
        {
            return Result.Failure(GamingErrors.DrawAlreadyExecuted);
        }

        draw.CloseManually(request.Reason, dateTimeProvider.UtcNow);
        drawRepository.Update(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
