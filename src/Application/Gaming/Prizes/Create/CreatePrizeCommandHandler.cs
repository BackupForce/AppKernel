using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Prizes;
using Domain.Gaming.Repositories;
using SharedKernel;

namespace Application.Gaming.Prizes.Create;

internal sealed class CreatePrizeCommandHandler(
    IPrizeRepository prizeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CreatePrizeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePrizeCommand request, CancellationToken cancellationToken)
    {
        Result<Prize> prizeResult = Prize.Create(
            tenantContext.TenantId,
            request.Name,
            request.Description,
            request.Cost,
            dateTimeProvider.UtcNow);

        if (prizeResult.IsFailure)
        {
            return Result.Failure<Guid>(prizeResult.Error);
        }

        Prize prize = prizeResult.Value;

        prizeRepository.Insert(prize);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return prize.Id;
    }
}
