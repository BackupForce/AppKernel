using SharedKernel;

namespace Domain.Gaming.DrawGroups.Events;

public sealed record DrawGroupEnabledDomainEvent(
    Guid TenantId,
    Guid DrawGroupId,
    Guid OperatorUserId,
    DateTime OccurredAtUtc,
    string? Reason) : IDomainEvent;
