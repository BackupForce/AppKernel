using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketClaimEvents.Disable;

public sealed record DisableTicketClaimEventCommand(Guid TenantId, Guid EventId) : ICommand;
