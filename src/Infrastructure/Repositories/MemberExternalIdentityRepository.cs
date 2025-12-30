using Domain.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberExternalIdentityRepository(ApplicationDbContext context) : IMemberExternalIdentityRepository
{
    public Task<MemberExternalIdentity?> GetByProviderAndExternalUserIdAsync(
        string provider,
        string externalUserId,
        CancellationToken cancellationToken = default)
    {
        return context.MemberExternalIdentities.FirstOrDefaultAsync(
            x => x.Provider == provider && x.ExternalUserId == externalUserId,
            cancellationToken);
    }

    public Task AddAsync(MemberExternalIdentity identity, CancellationToken cancellationToken = default)
    {
        return context.MemberExternalIdentities.AddAsync(identity, cancellationToken).AsTask();
    }

    public Task UpdateAsync(MemberExternalIdentity identity, CancellationToken cancellationToken = default)
    {
        context.MemberExternalIdentities.Update(identity);

        return Task.CompletedTask;
    }
}
