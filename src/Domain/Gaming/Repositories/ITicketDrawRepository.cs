using Domain.Gaming.Tickets;

namespace Domain.Gaming.Repositories;

public interface ITicketDrawRepository
{
    Task<IReadOnlyCollection<TicketDraw>> GetByTicketIdAsync(
        Guid tenantId,
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TicketDraw>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        TicketDrawParticipationStatus? status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TicketDraw>> GetPendingForUnsubmittedTicketsAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default);

    void Insert(TicketDraw ticketDraw);

    void Update(TicketDraw ticketDraw);

    void UpdateRange(IEnumerable<TicketDraw> ticketDraws);
}
