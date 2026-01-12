namespace Domain.Gaming;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid tenantId, Guid ticketId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Ticket>> GetByDrawIdAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken = default);

    void Insert(Ticket ticket);

    void InsertLine(TicketLine line);
}
