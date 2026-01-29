using Domain.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberProfileRepository(ApplicationDbContext context) : IMemberProfileRepository
{
    public Task<MemberProfile?> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return context.MemberProfiles.FirstOrDefaultAsync(profile => profile.MemberId == memberId, cancellationToken);
    }

    public void Insert(MemberProfile profile)
    {
        context.MemberProfiles.Add(profile);
    }

    public void Update(MemberProfile profile)
    {
        context.MemberProfiles.Update(profile);
    }
}
