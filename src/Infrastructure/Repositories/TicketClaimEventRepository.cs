using Domain.Gaming.Repositories;
using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketClaimEventRepository(ApplicationDbContext context) : ITicketClaimEventRepository
{
    public async Task<TicketClaimEvent?> GetByIdAsync(Guid tenantId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await context.TicketClaimEvents
            .FirstOrDefaultAsync(
                ticketClaimEvent => ticketClaimEvent.TenantId == tenantId && ticketClaimEvent.Id == eventId,
                cancellationToken);
    }

    public async Task<TicketClaimEvent?> GetByIdForUpdateAsync(Guid tenantId, Guid eventId, CancellationToken cancellationToken = default)
    {
        string sql = $"SELECT * FROM {Schemas.Gaming}.ticket_claim_events WHERE tenant_id = {{0}} AND id = {{1}} FOR UPDATE";

        return await context.TicketClaimEvents
            .FromSqlRaw(sql, tenantId, eventId)
            .AsTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Insert(TicketClaimEvent ticketClaimEvent)
    {
        context.TicketClaimEvents.Add(ticketClaimEvent);
    }

    public void Update(TicketClaimEvent ticketClaimEvent)
    {
        context.TicketClaimEvents.Update(ticketClaimEvent);
    }
}
