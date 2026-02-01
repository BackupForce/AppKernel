using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketClaimEvents.Activate;

public sealed record ActivateTicketClaimEventCommand(Guid TenantId, Guid EventId) : ICommand;
