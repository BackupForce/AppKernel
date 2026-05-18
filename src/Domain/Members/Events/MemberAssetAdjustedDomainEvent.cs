using SharedKernel;

namespace Domain.Members.Events;

public sealed record MemberAssetAdjustedDomainEvent(
    Guid MemberId,
    string AssetCode,
    MemberAssetLedgerType Type,
    decimal Amount,
    decimal BeforeBalance,
    decimal AfterBalance,
    Guid? OperatorUserId) : IDomainEvent;
