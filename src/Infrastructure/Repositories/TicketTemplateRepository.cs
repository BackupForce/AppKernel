using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketTemplateRepository(ApplicationDbContext context) : ITicketTemplateRepository
{
    public async Task<TicketTemplate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TicketTemplates
            .FirstOrDefaultAsync(template => template.TenantId == tenantId && template.Id == id, cancellationToken);
    }

    public async Task<TicketTemplate?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return await context.TicketTemplates
            .FirstOrDefaultAsync(template => template.TenantId == tenantId && template.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TicketTemplate>> GetListAsync(
        Guid tenantId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TicketTemplate> query = context.TicketTemplates
            .Where(template => template.TenantId == tenantId);

        if (activeOnly)
        {
            query = query.Where(template => template.IsActive);
        }

        List<TicketTemplate> result = await query
            .OrderBy(template => template.Code)
            .ToListAsync(cancellationToken);

        return result;
    }

    public void Insert(TicketTemplate template)
    {
        context.TicketTemplates.Add(template);
    }

    public void Update(TicketTemplate template)
    {
        context.TicketTemplates.Update(template);
    }
}
