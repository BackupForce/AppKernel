namespace Domain.Members;

public interface IMemberProfileRepository
{
    Task<MemberProfile?> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    void Insert(MemberProfile profile);

    void Update(MemberProfile profile);
}
