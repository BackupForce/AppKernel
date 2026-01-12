namespace Domain.Gaming;

/// <summary>
/// PrizeRule 儲存介面，由 Infrastructure 實作。
/// </summary>
public interface IPrizeRuleRepository
{
    /// <summary>
    /// 取得指定遊戲類型在指定時間內的有效規則。
    /// </summary>
    Task<IReadOnlyCollection<PrizeRule>> GetActiveRulesAsync(
        Guid tenantId,
        GameType gameType,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 判斷是否已有啟用中的規則（同 MatchCount），用於避免衝突。
    /// </summary>
    Task<bool> HasActiveRuleAsync(
        Guid tenantId,
        GameType gameType,
        int matchCount,
        Guid? excludeRuleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 依識別取得規則。
    /// </summary>
    Task<PrizeRule?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增規則。
    /// </summary>
    void Insert(PrizeRule rule);

    /// <summary>
    /// 更新規則。
    /// </summary>
    void Update(PrizeRule rule);
}
