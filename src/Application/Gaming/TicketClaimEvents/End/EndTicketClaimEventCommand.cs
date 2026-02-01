using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketClaimEvents.End;

public sealed record EndTicketClaimEventCommand(Guid TenantId, Guid EventId) : ICommand;
