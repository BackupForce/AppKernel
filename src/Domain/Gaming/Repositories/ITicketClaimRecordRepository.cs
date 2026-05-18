using Domain.Gaming.TicketClaimEvents;

namespace Domain.Gaming.Repositories;

public interface ITicketClaimRecordRepository
{
    Task<TicketClaimRecord?> GetByIdempotencyKeyAsync(
        Guid tenantId,
        Guid eventId,
        Guid memberId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    void Insert(TicketClaimRecord record);
}
