using Domain.Gaming.PrizeAwards;

namespace Domain.Gaming.Repositories;

/// <summary>
/// PrizeAward 儲存介面，由 Infrastructure 實作。
/// </summary>
public interface IPrizeAwardRepository
{
    /// <summary>
    /// 依識別取得得獎記錄。
    /// </summary>
    Task<PrizeAward?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 防重檢查：指定期數、票券與注數是否已建立 Award。
    /// </summary>
    Task<bool> ExistsAsync(
        Guid tenantId,
        Guid drawId,
        Guid ticketId,
        int lineIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增得獎記錄。
    /// </summary>
    void Insert(PrizeAward prizeAward);

    /// <summary>
    /// 更新得獎記錄。
    /// </summary>
    void Update(PrizeAward prizeAward);
}
