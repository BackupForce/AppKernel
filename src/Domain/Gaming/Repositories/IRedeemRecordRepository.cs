using Domain.Gaming.RedeemRecords;

namespace Domain.Gaming.Repositories;

/// <summary>
/// RedeemRecord 儲存介面，由 Infrastructure 實作。
/// </summary>
public interface IRedeemRecordRepository
{
    /// <summary>
    /// 依得獎記錄檢查是否已有兌換紀錄，做為防重保護。
    /// </summary>
    Task<bool> ExistsAsync(Guid prizeAwardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 依得獎記錄取得兌換紀錄，用於 idempotency。
    /// </summary>
    Task<RedeemRecord?> GetByAwardIdAsync(Guid prizeAwardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增兌換紀錄。
    /// </summary>
    void Insert(RedeemRecord record);
}
