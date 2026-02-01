using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Activate;

internal sealed class ActivateTicketClaimEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivateTicketClaimEventCommand>
{
    public async Task<Result> Handle(ActivateTicketClaimEventCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.TicketClaimEventTenantMismatch);
        }

        var ticketClaimEvent = await ticketClaimEventRepository.GetByIdAsync(
            request.TenantId,
            request.EventId,
            cancellationToken);

        if (ticketClaimEvent is null)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotFound);
        }

        Result activateResult = ticketClaimEvent.Activate(dateTimeProvider.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        ticketClaimEventRepository.Update(ticketClaimEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
