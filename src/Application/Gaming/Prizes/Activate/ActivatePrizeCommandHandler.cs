using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Prizes;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Prizes.Activate;

internal sealed class ActivatePrizeCommandHandler(
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivatePrizeCommand>
{
    public async Task<Result> Handle(ActivatePrizeCommand request, CancellationToken cancellationToken)
    {
        Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, request.PrizeId, cancellationToken);
        if (prize is null)
        {
            return Result.Failure(GamingErrors.PrizeNotFound);
        }

        prize.Activate(dateTimeProvider.UtcNow);
        prizeRepository.Update(prize);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
