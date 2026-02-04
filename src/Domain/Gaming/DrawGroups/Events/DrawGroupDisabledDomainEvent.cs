using SharedKernel;

namespace Domain.Gaming.DrawGroups.Events;

public sealed record DrawGroupDisabledDomainEvent(
    Guid TenantId,
    Guid DrawGroupId,
    Guid OperatorUserId,
    DateTime OccurredAtUtc,
    string? Reason) : IDomainEvent;
