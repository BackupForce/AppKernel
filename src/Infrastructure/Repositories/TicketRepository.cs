using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketRepository(ApplicationDbContext context) : ITicketRepository
{
    public async Task<Ticket?> GetByIdAsync(Guid tenantId, Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Tickets
            .Include(ticket => ticket.Lines)
            .FirstOrDefaultAsync(ticket => ticket.TenantId == tenantId && ticket.Id == ticketId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Ticket>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default)
    {
        List<Ticket> tickets = await context.Tickets
            .Include(ticket => ticket.Lines)
            .Where(ticket => ticket.TenantId == tenantId && ticket.DrawId == drawId)
            .ToListAsync(cancellationToken);

        return tickets;
    }

    public void Insert(Ticket ticket)
    {
        context.Tickets.Add(ticket);
    }

    public void InsertLine(TicketLine line)
    {
        context.TicketLines.Add(line);
    }
}
