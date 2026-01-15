using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Prizes;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Prizes.Update;

internal sealed class UpdatePrizeCommandHandler(
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdatePrizeCommand>
{
    public async Task<Result> Handle(UpdatePrizeCommand request, CancellationToken cancellationToken)
    {
        Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, request.PrizeId, cancellationToken);
        if (prize is null)
        {
            return Result.Failure(GamingErrors.PrizeNotFound);
        }

        prize.Update(request.Name, request.Description, request.Cost, dateTimeProvider.UtcNow);
        prizeRepository.Update(prize);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
