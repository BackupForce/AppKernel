namespace Domain.Members;

public interface IMemberExternalIdentityRepository
{
    Task<MemberExternalIdentity?> GetByProviderAndExternalUserIdAsync(
        string provider,
        string externalUserId,
        CancellationToken cancellationToken = default);

    Task AddAsync(MemberExternalIdentity identity, CancellationToken cancellationToken = default);

    Task UpdateAsync(MemberExternalIdentity identity, CancellationToken cancellationToken = default);
}
