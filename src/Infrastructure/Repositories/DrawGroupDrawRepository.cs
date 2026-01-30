using Domain.Gaming.DrawGroups;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class DrawGroupDrawRepository(ApplicationDbContext context) : IDrawGroupDrawRepository
{
    public async Task<IReadOnlyCollection<DrawGroupDraw>> GetByDrawGroupIdAsync(
        Guid tenantId,
        Guid drawGroupId,
        CancellationToken cancellationToken = default)
    {
        return await context.DrawGroupDraws
            .Where(item => item.TenantId == tenantId && item.DrawGroupId == drawGroupId)
            .ToListAsync(cancellationToken);
    }
}
