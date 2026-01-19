using Domain.Gaming.Repositories;
using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketDrawRepository(ApplicationDbContext context) : ITicketDrawRepository
{
    public async Task<IReadOnlyCollection<TicketDraw>> GetByTicketIdAsync(
        Guid tenantId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketDraws
            .Where(item => item.TenantId == tenantId && item.TicketId == ticketId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TicketDraw>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        TicketDrawParticipationStatus? status,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TicketDraw> query = context.TicketDraws
            .Where(item => item.TenantId == tenantId && item.DrawId == drawId);

        if (status.HasValue)
        {
            query = query.Where(item => item.ParticipationStatus == status.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TicketDraw>> GetPendingForUnsubmittedTicketsAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketDraws
            .Where(item => item.TenantId == tenantId
                && item.DrawId == drawId
                && item.ParticipationStatus == TicketDrawParticipationStatus.Pending
                && context.Tickets.Any(
                    ticket => ticket.TenantId == tenantId
                        && ticket.Id == item.TicketId
                        && ticket.SubmissionStatus == TicketSubmissionStatus.NotSubmitted))
            .ToListAsync(cancellationToken);
    }

    public void Insert(TicketDraw ticketDraw)
    {
        context.TicketDraws.Add(ticketDraw);
    }

    public void Update(TicketDraw ticketDraw)
    {
        context.TicketDraws.Update(ticketDraw);
    }

    public void UpdateRange(IEnumerable<TicketDraw> ticketDraws)
    {
        context.TicketDraws.UpdateRange(ticketDraws);
    }
}
