using Domain.Gaming.Tickets;

namespace Domain.Gaming.Repositories;

public interface ITicketIdempotencyRepository
{
    Task<TicketIdempotencyRecord?> GetByKeyAsync(
        Guid tenantId,
        string idempotencyKey,
        string operation,
        CancellationToken cancellationToken = default);

    void Insert(TicketIdempotencyRecord record);
}
