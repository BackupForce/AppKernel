namespace Domain.Gaming;

/// <summary>
/// TicketTemplate 儲存介面，由 Infrastructure 實作。
/// </summary>
public interface ITicketTemplateRepository
{
    Task<TicketTemplate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    Task<TicketTemplate?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TicketTemplate>> GetListAsync(Guid tenantId, bool activeOnly, CancellationToken cancellationToken = default);

    void Insert(TicketTemplate template);

    void Update(TicketTemplate template);
}
