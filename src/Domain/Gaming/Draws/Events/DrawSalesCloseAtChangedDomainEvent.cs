using SharedKernel;

namespace Domain.Gaming.Draws.Events;

public sealed record DrawSalesCloseAtChangedDomainEvent(
    Guid TenantId,
    Guid DrawId,
    DateTime OccurredAtUtc) : IDomainEvent;
