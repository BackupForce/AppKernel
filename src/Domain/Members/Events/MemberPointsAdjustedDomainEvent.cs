using SharedKernel;

namespace Domain.Members.Events;

public sealed record MemberPointsAdjustedDomainEvent(
    Guid MemberId,
    MemberPointLedgerType Type,
    long Amount,
    long BeforeBalance,
    long AfterBalance,
    Guid? OperatorUserId) : IDomainEvent;
