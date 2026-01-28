using Domain.Gaming.DrawTemplates;

namespace Domain.Gaming.Repositories;

/// <summary>
/// DrawTemplate 聚合的儲存介面。
/// </summary>
public interface IDrawTemplateRepository
{
    Task<DrawTemplate?> GetByIdAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);

    Task<DrawTemplate?> GetByNameAsync(
        Guid tenantId,
        string gameCode,
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> HasDrawsAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken = default);

    void Insert(DrawTemplate template);

    void Update(DrawTemplate template);
}
