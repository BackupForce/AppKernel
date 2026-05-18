using Domain.Gaming.TicketClaimEvents;

namespace Domain.Gaming.Repositories;

public interface ITicketClaimMemberCounterRepository
{
    Task<TicketClaimMemberCounter?> GetByIdAsync(Guid eventId, Guid memberId, CancellationToken cancellationToken = default);

    Task<TicketClaimMemberCounter?> GetByIdForUpdateAsync(Guid eventId, Guid memberId, CancellationToken cancellationToken = default);

    void Insert(TicketClaimMemberCounter counter);

    void Update(TicketClaimMemberCounter counter);
}
