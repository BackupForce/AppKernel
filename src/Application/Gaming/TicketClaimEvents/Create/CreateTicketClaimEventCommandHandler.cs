using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketClaimEvents;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Create;

internal sealed class CreateTicketClaimEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CreateTicketClaimEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketClaimEventCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<Guid>(GamingErrors.TicketClaimEventTenantMismatch);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result<TicketClaimEvent> createResult = TicketClaimEvent.Create(
            request.TenantId,
            request.Name,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.TotalQuota,
            request.PerMemberQuota,
            request.ScopeType,
            request.ScopeId,
            request.TicketTemplateId,
            now);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        ticketClaimEventRepository.Insert(createResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return createResult.Value.Id;
    }
}
