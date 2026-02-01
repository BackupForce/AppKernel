using Domain.Gaming.Repositories;
using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketClaimMemberCounterRepository(ApplicationDbContext context) : ITicketClaimMemberCounterRepository
{
    public async Task<TicketClaimMemberCounter?> GetByIdAsync(Guid eventId, Guid memberId, CancellationToken cancellationToken = default)
    {
        return await context.TicketClaimMemberCounters
            .FirstOrDefaultAsync(counter => counter.EventId == eventId && counter.MemberId == memberId, cancellationToken);
    }

    public async Task<TicketClaimMemberCounter?> GetByIdForUpdateAsync(Guid eventId, Guid memberId, CancellationToken cancellationToken = default)
    {
        string sql = $"SELECT * FROM {Schemas.Gaming}.ticket_claim_member_counters WHERE event_id = {{0}} AND member_id = {{1}} FOR UPDATE";

        return await context.TicketClaimMemberCounters
            .FromSqlRaw(sql, eventId, memberId)
            .AsTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Insert(TicketClaimMemberCounter counter)
    {
        context.TicketClaimMemberCounters.Add(counter);
    }

    public void Update(TicketClaimMemberCounter counter)
    {
        context.TicketClaimMemberCounters.Update(counter);
    }
}
