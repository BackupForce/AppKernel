using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class DrawAllowedTicketTemplateRepository(ApplicationDbContext context)
    : IDrawAllowedTicketTemplateRepository
{
    public async Task<IReadOnlyCollection<DrawAllowedTicketTemplate>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default)
    {
        List<DrawAllowedTicketTemplate> items = await context.DrawAllowedTicketTemplates
            .Where(item => item.TenantId == tenantId && item.DrawId == drawId)
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<bool> ExistsAsync(
        Guid tenantId,
        Guid drawId,
        Guid ticketTemplateId,
        CancellationToken cancellationToken = default)
    {
        return await context.DrawAllowedTicketTemplates
            .AnyAsync(
                item => item.TenantId == tenantId
                    && item.DrawId == drawId
                    && item.TicketTemplateId == ticketTemplateId,
                cancellationToken);
    }

    public void Insert(DrawAllowedTicketTemplate item)
    {
        context.DrawAllowedTicketTemplates.Add(item);
    }

    public void RemoveRange(IEnumerable<DrawAllowedTicketTemplate> items)
    {
        context.DrawAllowedTicketTemplates.RemoveRange(items);
    }
}
