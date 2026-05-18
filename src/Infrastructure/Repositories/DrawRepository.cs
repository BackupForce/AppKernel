using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class DrawRepository(ApplicationDbContext context) : IDrawRepository
{
    public async Task<Draw?> GetByIdAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken = default)
    {
        return await context.Draws
            .Include(draw => draw.EnabledPlayTypeItems)
            .Include(draw => draw.PrizePoolItems)
            .FirstOrDefaultAsync(draw => draw.TenantId == tenantId && draw.Id == drawId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Draw>> GetByIdsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default)
    {
        if (drawIds.Count == 0)
        {
            return Array.Empty<Draw>();
        }

        return await context.Draws
            .Where(draw => draw.TenantId == tenantId && drawIds.Contains(draw.Id))
            .ToListAsync(cancellationToken);
    }

    public void Insert(Draw draw)
    {
        context.Draws.Add(draw);
    }

    public void Update(Draw draw)
    {
        context.Draws.Update(draw);
    }
}
