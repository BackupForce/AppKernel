using Domain.Gaming.Repositories;
using Domain.Gaming.Tickets;
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

    public async Task<IReadOnlyCollection<Ticket>> GetByIdsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> ticketIds,
        CancellationToken cancellationToken = default)
    {
        List<Ticket> tickets = await context.Tickets
            .Include(ticket => ticket.Lines)
            .Where(ticket => ticket.TenantId == tenantId && ticketIds.Contains(ticket.Id))
            .ToListAsync(cancellationToken);

        return tickets;
    }

    public async Task<IReadOnlyCollection<Ticket>> GetByMemberIdAsync(
    Guid tenantId,
    Guid memberId,
    DateTime? from,
    DateTime? to,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Ticket> query = context.Tickets
            .Include(ticket => ticket.Lines)
            .Where(ticket => ticket.TenantId == tenantId && ticket.MemberId == memberId);

        if (from.HasValue)
        {
            // 這裡假設 Ticket 有 CreatedAt 或 PurchasedAt 之類時間欄位
            query = query.Where(ticket => ticket.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(ticket => ticket.CreatedAt <= to.Value);
        }

        List<Ticket> tickets = await query.ToListAsync(cancellationToken);
        return tickets;
    }

    public async Task<bool> ExistsForDrawGroupAsync(
        Guid tenantId,
        Guid memberId,
        Guid drawGroupId,
        CancellationToken cancellationToken = default)
    {
        return await context.Tickets.AnyAsync(
            ticket => ticket.TenantId == tenantId && ticket.MemberId == memberId && ticket.DrawGroupId == drawGroupId,
            cancellationToken);
    }

    public async Task<bool> TryMarkSubmittedAsync(
        Guid tenantId,
        Guid ticketId,
        DateTime submittedAtUtc,
        Guid? submittedByUserId,
        string? clientReference,
        string? note,
        CancellationToken cancellationToken = default)
    {
        int affected = await context.Tickets
            .Where(ticket => ticket.TenantId == tenantId
                             && ticket.Id == ticketId
                             && ticket.SubmissionStatus == TicketSubmissionStatus.NotSubmitted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(ticket => ticket.SubmissionStatus, TicketSubmissionStatus.Submitted)
                    .SetProperty(ticket => ticket.SubmittedAtUtc, submittedAtUtc)
                    .SetProperty(ticket => ticket.SubmittedByUserId, submittedByUserId)
                    .SetProperty(ticket => ticket.SubmittedClientReference, clientReference)
                    .SetProperty(ticket => ticket.SubmittedNote, note),
                cancellationToken);

        return affected == 1;
    }

    public void Insert(Ticket ticket)
    {
        context.Tickets.Add(ticket);
    }

    public void Update(Ticket ticket)
    {
        context.Tickets.Update(ticket);
    }

    public void InsertLine(TicketLine line)
    {
        context.TicketLines.Add(line);
    }
}
