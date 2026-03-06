using Domain.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberTagBindingRepository(ApplicationDbContext context) : IMemberTagBindingRepository
{
    public async Task<IReadOnlyCollection<Guid>> GetTagIdsByMemberIdAsync(
        Guid tenantId,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return await context.MemberTagBindings
            .Where(binding => binding.TenantId == tenantId && binding.MemberId == memberId)
            .Select(binding => binding.TagId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceMemberTagsAsync(
        Guid tenantId,
        Guid memberId,
        IReadOnlyCollection<Guid> tagIds,
        Guid? operatorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        List<MemberTagBinding> existing = await context.MemberTagBindings
            .Where(binding => binding.TenantId == tenantId && binding.MemberId == memberId)
            .ToListAsync(cancellationToken);

        context.MemberTagBindings.RemoveRange(existing);

        foreach (Guid tagId in tagIds.Distinct())
        {
            context.MemberTagBindings.Add(MemberTagBinding.Create(tenantId, memberId, tagId, nowUtc, operatorUserId));
        }
    }
}
