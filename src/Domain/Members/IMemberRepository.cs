namespace Domain.Members;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid tenantId, Guid memberId, CancellationToken cancellationToken = default);

    Task<Member?> GetByMemberNoAsync(Guid tenantId, string memberNo, CancellationToken cancellationToken = default);

    Task<Member?> GetByUserIdAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsMemberNoUniqueAsync(Guid tenantId, string memberNo, CancellationToken cancellationToken = default);

    Task<bool> IsUserIdUniqueAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    Task<MemberPointBalance?> GetPointBalanceAsync(Guid memberId, CancellationToken cancellationToken = default);

    Task<MemberAssetBalance?> GetAssetBalanceAsync(
        Guid memberId,
        string assetCode,
        CancellationToken cancellationToken = default);

    void Insert(Member member);

    void InsertPointBalance(MemberPointBalance balance);
    void UpsertPointBalance(MemberPointBalance balance);

    void UpsertAssetBalance(MemberAssetBalance balance);

    void InsertPointLedger(MemberPointLedger ledger);

    void InsertAssetLedger(MemberAssetLedger ledger);

    void InsertActivity(MemberActivityLog log);
}
