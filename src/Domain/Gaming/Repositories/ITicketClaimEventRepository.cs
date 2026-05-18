using Domain.Gaming.TicketClaimEvents;

namespace Domain.Gaming.Repositories;

public interface ITicketClaimEventRepository
{
    Task<TicketClaimEvent?> GetByIdAsync(Guid tenantId, Guid eventId, CancellationToken cancellationToken = default);

    Task<TicketClaimEvent?> GetByIdForUpdateAsync(Guid tenantId, Guid eventId, CancellationToken cancellationToken = default);

    void Insert(TicketClaimEvent ticketClaimEvent);

    void Update(TicketClaimEvent ticketClaimEvent);
}
