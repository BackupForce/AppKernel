using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Update;

internal sealed class UpdateTicketClaimEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdateTicketClaimEventCommand>
{
    public async Task<Result> Handle(UpdateTicketClaimEventCommand request, CancellationToken cancellationToken)
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

        Result updateResult = ticketClaimEvent.UpdateInfo(
            request.Name,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.TotalQuota,
            request.PerMemberQuota,
            request.ScopeType,
            request.ScopeId,
            request.TicketTemplateId,
            dateTimeProvider.UtcNow);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        ticketClaimEventRepository.Update(ticketClaimEvent);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
