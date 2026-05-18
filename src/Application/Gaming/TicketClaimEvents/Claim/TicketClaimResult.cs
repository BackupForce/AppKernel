namespace Application.Gaming.TicketClaimEvents.Claim;

public sealed record TicketClaimResult(Guid EventId, IReadOnlyCollection<Guid> TicketIds, int Quantity);
