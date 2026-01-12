namespace Domain.Gaming;

/// <summary>
/// 期數獎項對應設定的儲存介面。
/// </summary>
public interface IDrawPrizeMappingRepository
{
    Task<IReadOnlyCollection<DrawPrizeMapping>> GetByDrawIdAsync(
        Guid tenantId,
        Guid drawId,
        CancellationToken cancellationToken = default);

    void Insert(DrawPrizeMapping mapping);

    void RemoveRange(IEnumerable<DrawPrizeMapping> mappings);
}
