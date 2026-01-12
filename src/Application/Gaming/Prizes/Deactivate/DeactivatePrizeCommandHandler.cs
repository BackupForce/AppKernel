using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using SharedKernel;

namespace Application.Gaming.Prizes.Deactivate;

internal sealed class DeactivatePrizeCommandHandler(
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<DeactivatePrizeCommand>
{
    public async Task<Result> Handle(DeactivatePrizeCommand request, CancellationToken cancellationToken)
    {
        Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, request.PrizeId, cancellationToken);
        if (prize is null)
        {
            return Result.Failure(GamingErrors.PrizeNotFound);
        }

        prize.Deactivate(dateTimeProvider.UtcNow);
        prizeRepository.Update(prize);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
