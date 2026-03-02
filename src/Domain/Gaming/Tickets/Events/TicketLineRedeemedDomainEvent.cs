using SharedKernel;

namespace Domain.Gaming.Tickets.Events;

public sealed record TicketLineRedeemedDomainEvent(
    Guid TicketLineResultId,
    Guid TicketId,
    Guid DrawId,
    Guid RedeemedByUserId,
    DateTime RedeemedAtUtc) : IDomainEvent;
