using Domain.Gaming.Repositories;
using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketClaimRecordRepository(ApplicationDbContext context) : ITicketClaimRecordRepository
{
    public async Task<TicketClaimRecord?> GetByIdempotencyKeyAsync(
        Guid tenantId,
        Guid eventId,
        Guid memberId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketClaimRecords
            .FirstOrDefaultAsync(
                record => record.TenantId == tenantId
                          && record.EventId == eventId
                          && record.MemberId == memberId
                          && record.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    public void Insert(TicketClaimRecord record)
    {
        context.TicketClaimRecords.Add(record);
    }
}
