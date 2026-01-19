using Domain.Gaming.Tickets;

namespace Domain.Gaming.Repositories;

/// <summary>
/// Ticket 聚合的儲存介面，由 Infrastructure 提供實作。
/// </summary>
public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid tenantId, Guid ticketId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Ticket>> GetByIdsAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> ticketIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Ticket>> GetByMemberIdAsync(
        Guid tenantId,
        Guid memberId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForCampaignAsync(
        Guid tenantId,
        Guid memberId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    void Insert(Ticket ticket);

    void Update(Ticket ticket);

    void InsertLine(TicketLine line);
}
