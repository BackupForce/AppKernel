using Domain.Gaming.Repositories;
using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketLineResultRepository(ApplicationDbContext context) : ITicketLineResultRepository
{
    public async Task<bool> ExistsAsync(
        Guid tenantId,
        Guid ticketId,
        Guid drawId,
        int lineIndex,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketLineResults.AnyAsync(
            result => result.TenantId == tenantId
                && result.TicketId == ticketId
                && result.DrawId == drawId
                && result.LineIndex == lineIndex,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<TicketLineResult>> GetByDrawAndTicketsAsync(
        Guid tenantId,
        Guid drawId,
        IReadOnlyCollection<Guid> ticketIds,
        CancellationToken cancellationToken = default)
    {
        if (ticketIds.Count == 0)
        {
            return Array.Empty<TicketLineResult>();
        }

        return await context.TicketLineResults
            .AsNoTracking()
            .Where(result => result.TenantId == tenantId
                && result.DrawId == drawId
                && ticketIds.Contains(result.TicketId))
            .ToListAsync(cancellationToken);
    }

    public void Insert(TicketLineResult result)
    {
        context.TicketLineResults.Add(result);
    }
}
