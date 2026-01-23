using Domain.Gaming.Repositories;
using Domain.Gaming.Tickets;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketIdempotencyRepository(ApplicationDbContext context) : ITicketIdempotencyRepository
{
    public async Task<TicketIdempotencyRecord?> GetByKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        string operation,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketIdempotencyRecords.FirstOrDefaultAsync(
            record => record.TenantId == tenantId
                      && record.IdempotencyKey == idempotencyKey
                      && record.Operation == operation,
            cancellationToken);
    }

    public void Insert(TicketIdempotencyRecord record)
    {
        context.TicketIdempotencyRecords.Add(record);
    }
}
