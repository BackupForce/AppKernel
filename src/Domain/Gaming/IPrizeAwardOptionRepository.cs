namespace Domain.Gaming;

/// <summary>
/// 兌獎選項快照儲存介面。
/// </summary>
public interface IPrizeAwardOptionRepository
{
    Task<IReadOnlyCollection<PrizeAwardOption>> GetByAwardIdAsync(
        Guid tenantId,
        Guid awardId,
        CancellationToken cancellationToken = default);

    void InsertRange(IEnumerable<PrizeAwardOption> options);
}
