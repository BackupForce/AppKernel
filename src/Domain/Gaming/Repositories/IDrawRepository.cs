using Domain.Gaming.Draws;

namespace Domain.Gaming.Repositories;

/// <summary>
/// Draw 聚合的儲存介面，由 Infrastructure 提供實作。
/// </summary>
public interface IDrawRepository
{
    /// <summary>
    /// 依租戶與期數取得 Draw。
    /// </summary>
    Task<Draw?> GetByIdAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增 Draw。
    /// </summary>
    void Insert(Draw draw);

    /// <summary>
    /// 更新 Draw。
    /// </summary>
    void Update(Draw draw);
}
