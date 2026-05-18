using Domain.Gaming.Prizes;

namespace Domain.Gaming.Repositories;

/// <summary>
/// Prize 儲存介面，由 Infrastructure 實作。
/// </summary>
public interface IPrizeRepository
{
    /// <summary>
    /// 依識別取得獎品。
    /// </summary>
    Task<Prize?> GetByIdAsync(Guid tenantId, Guid prizeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 檢查獎品名稱是否唯一。
    /// </summary>
    Task<bool> IsNameUniqueAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增獎品。
    /// </summary>
    void Insert(Prize prize);

    /// <summary>
    /// 更新獎品。
    /// </summary>
    void Update(Prize prize);
}
