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

    public void Insert(Draw draw)
    {
        context.Draws.Add(draw);
    }

    public void Update(Draw draw)
    {
        context.Draws.Update(draw);
    }
}
