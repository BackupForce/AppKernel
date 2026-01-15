using Domain.Gaming.Tickets;

namespace Domain.Gaming.Repositories;

/// <summary>
/// Ticket 聚合的儲存介面，由 Infrastructure 提供實作。
/// </summary>
public interface ITicketRepository
{
    /// <summary>
    /// 依期數取得所有票券（結算用）。
    /// </summary>
    Task<IReadOnlyCollection<Ticket>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 依會員取得票券（查詢用）。
    /// </summary>
    Task<IReadOnlyCollection<Ticket>> GetByMemberIdAsync(
        Guid tenantId,
        Guid memberId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增票券。
    /// </summary>
    void Insert(Ticket ticket);

    /// <summary>
    /// 新增票券中的單注。
    /// </summary>
    void InsertLine(TicketLine line);
}
