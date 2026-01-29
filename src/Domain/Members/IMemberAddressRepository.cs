namespace Domain.Members;

public interface IMemberAddressRepository
{
    Task<MemberAddress?> GetByIdAsync(Guid memberId, Guid id, CancellationToken cancellationToken = default);

    Task<List<MemberAddress>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    void Insert(MemberAddress address);

    void Update(MemberAddress address);

    void Remove(MemberAddress address);
}
