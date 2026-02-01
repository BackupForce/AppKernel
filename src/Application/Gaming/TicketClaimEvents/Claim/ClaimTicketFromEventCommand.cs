using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketClaimEvents.Claim;

public sealed record ClaimTicketFromEventCommand(Guid EventId, string? IdempotencyKey) : ICommand<TicketClaimResult>;
