using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class DrawPrizeMappingRepository(ApplicationDbContext context) : IDrawPrizeMappingRepository
{
    public async Task<IReadOnlyCollection<DrawPrizeMapping>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default)
    {
        List<DrawPrizeMapping> items = await context.DrawPrizeMappings
            .Where(item => item.TenantId == tenantId && item.DrawId == drawId)
            .ToListAsync(cancellationToken);

        return items;
    }

    public void Insert(DrawPrizeMapping mapping)
    {
        context.DrawPrizeMappings.Add(mapping);
    }

    public void RemoveRange(IEnumerable<DrawPrizeMapping> mappings)
    {
        context.DrawPrizeMappings.RemoveRange(mappings);
    }
}
