using Domain.Gaming.Repositories;
using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketLineResultRepository(ApplicationDbContext context) : ITicketLineResultRepository
{
    public async Task<TicketLineResult?> GetByIdAsync(
        Guid tenantId,
        Guid ticketLineResultId,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketLineResults
            .FirstOrDefaultAsync(
                result => result.TenantId == tenantId && result.Id == ticketLineResultId,
                cancellationToken);
    }

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

    public async Task DeleteByDrawIdAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken = default)
    {
        List<TicketLineResult> results = await context.TicketLineResults
            .Where(result => result.TenantId == tenantId && result.DrawId == drawId)
            .ToListAsync(cancellationToken);

        if (results.Count == 0)
        {
            return;
        }

        context.TicketLineResults.RemoveRange(results);
    }

    public void Insert(TicketLineResult result)
    {
        context.TicketLineResults.Add(result);
    }

    public void Update(TicketLineResult result)
    {
        context.TicketLineResults.Update(result);
    }
}
