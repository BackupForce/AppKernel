using Domain.Gaming.Tickets;

namespace Domain.Gaming.Repositories;

public interface ITicketLineResultRepository
{
    Task<bool> ExistsAsync(
        Guid tenantId,
        Guid ticketId,
        Guid drawId,
        int lineIndex,
        CancellationToken cancellationToken = default);

    void Insert(TicketLineResult result);
}
