using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.End;

internal sealed class EndTicketClaimEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<EndTicketClaimEventCommand>
{
    public async Task<Result> Handle(EndTicketClaimEventCommand request, CancellationToken cancellationToken)
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

        Result endResult = ticketClaimEvent.End(dateTimeProvider.UtcNow);
        if (endResult.IsFailure)
        {
            return endResult;
        }

        ticketClaimEventRepository.Update(ticketClaimEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
