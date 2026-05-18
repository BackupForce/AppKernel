using Domain.Gaming.Tickets;

namespace Domain.Gaming.Repositories;

public interface ITicketLineResultRepository
{
    Task<TicketLineResult?> GetByIdAsync(
        Guid tenantId,
        Guid ticketLineResultId,
        CancellationToken cancellationToken = default);

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

    Task DeleteByDrawIdAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken = default);

    void Insert(TicketLineResult result);

    void Update(TicketLineResult result);
}
