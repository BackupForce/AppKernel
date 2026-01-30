using Domain.Gaming.DrawGroups;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class DrawGroupRepository(ApplicationDbContext context) : IDrawGroupRepository
{
    public async Task<DrawGroup?> GetByIdAsync(Guid tenantId, Guid drawGroupId, CancellationToken cancellationToken = default)
    {
        return await context.DrawGroups
            .Include(drawGroup => drawGroup.Draws)
            .FirstOrDefaultAsync(
                drawGroup => drawGroup.TenantId == tenantId && drawGroup.Id == drawGroupId,
                cancellationToken);
    }

    public void Insert(DrawGroup drawGroup)
    {
        context.DrawGroups.Add(drawGroup);
    }

    public void Update(DrawGroup drawGroup)
    {
        context.DrawGroups.Update(drawGroup);
    }

    public void Remove(DrawGroup drawGroup)
    {
        context.DrawGroups.Remove(drawGroup);
    }
}
