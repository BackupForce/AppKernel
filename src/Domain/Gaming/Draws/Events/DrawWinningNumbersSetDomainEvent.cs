using SharedKernel;

namespace Domain.Gaming.Draws.Events;

public sealed record DrawWinningNumbersSetDomainEvent(
    Guid TenantId,
    Guid DrawId,
    Guid OperatorUserId,
    DateTime OccurredAtUtc,
    string? SourceNote,
    bool IsRecalculation) : IDomainEvent;
