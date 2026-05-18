using Domain.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberTagCatalogRepository(ApplicationDbContext context) : IMemberTagCatalogRepository
{
    public Task<MemberTag?> GetByIdAsync(
        Guid tenantId,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        return context.MemberTags.FirstOrDefaultAsync(tag => tag.TenantId == tenantId && tag.Id == tagId, cancellationToken);
    }

    public Task<MemberTag?> GetByCodeAsync(
        Guid tenantId,
        string tagCode,
        CancellationToken cancellationToken = default)
    {
        return context.MemberTags.FirstOrDefaultAsync(tag => tag.TenantId == tenantId && tag.TagCode == tagCode, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MemberTag>> GetByIdsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken = default)
    {
        if (tagIds.Count == 0)
        {
            return Array.Empty<MemberTag>();
        }

        return await context.MemberTags
            .Where(tag => tag.TenantId == tenantId && tagIds.Contains(tag.Id))
            .ToListAsync(cancellationToken);
    }

    public void Insert(MemberTag tag)
    {
        context.MemberTags.Add(tag);
    }

    public void Update(MemberTag tag)
    {
        context.MemberTags.Update(tag);
    }
}
