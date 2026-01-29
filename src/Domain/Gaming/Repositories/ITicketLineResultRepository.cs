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

    Task<IReadOnlyCollection<TicketLineResult>> GetByDrawAndTicketsAsync(
        Guid tenantId,
        Guid drawId,
        IReadOnlyCollection<Guid> ticketIds,
        CancellationToken cancellationToken = default);

    void Insert(TicketLineResult result);
}
