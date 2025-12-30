namespace Domain.Members;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    Task<Member?> GetByMemberNoAsync(string memberNo, CancellationToken cancellationToken = default);

    Task<bool> IsMemberNoUniqueAsync(string memberNo, CancellationToken cancellationToken = default);

    Task<bool> IsUserIdUniqueAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<MemberPointBalance?> GetPointBalanceAsync(Guid memberId, CancellationToken cancellationToken = default);

    Task<MemberAssetBalance?> GetAssetBalanceAsync(
        Guid memberId,
        string assetCode,
        CancellationToken cancellationToken = default);

    void Insert(Member member);

    void UpsertPointBalance(MemberPointBalance balance);

    void UpsertAssetBalance(MemberAssetBalance balance);

    void InsertPointLedger(MemberPointLedger ledger);

    void InsertAssetLedger(MemberAssetLedger ledger);

    void InsertActivity(MemberActivityLog log);
}
