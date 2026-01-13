namespace Domain.Gaming;

/// <summary>
/// 期數允許票種設定的儲存介面。
/// </summary>
public interface IDrawAllowedTicketTemplateRepository
{
    Task<IReadOnlyCollection<DrawAllowedTicketTemplate>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid tenantId,
        Guid drawId,
        Guid ticketTemplateId,
        CancellationToken cancellationToken = default);

    void Insert(DrawAllowedTicketTemplate item);

    void RemoveRange(IEnumerable<DrawAllowedTicketTemplate> items);
}
