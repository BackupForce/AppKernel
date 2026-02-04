using SharedKernel;

namespace Domain.Gaming.Draws.Events;

public sealed record DrawManuallyClosedDomainEvent(
    Guid TenantId,
    Guid DrawId,
    DateTime OccurredAtUtc) : IDomainEvent;
