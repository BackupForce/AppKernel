using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 兌獎選項快照，避免後台改動影響歷史兌獎。
/// </summary>
[Obsolete("PrizeAwardOption 已改為 PrizeAward 內建快照。")]
public sealed class PrizeAwardOption : Entity
{
    private PrizeAwardOption(
        Guid id,
        Guid tenantId,
        Guid prizeAwardId,
        Guid prizeId,
        string prizeNameSnapshot,
        decimal prizeCostSnapshot,
        DateTime createdAt) : base(id)
    {
        TenantId = tenantId;
        PrizeAwardId = prizeAwardId;
        PrizeId = prizeId;
        PrizeNameSnapshot = prizeNameSnapshot;
        PrizeCostSnapshot = prizeCostSnapshot;
        CreatedAt = createdAt;
    }

    private PrizeAwardOption()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid PrizeAwardId { get; private set; }

    public Guid PrizeId { get; private set; }

    public string PrizeNameSnapshot { get; private set; } = string.Empty;

    public decimal PrizeCostSnapshot { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static PrizeAwardOption Create(
        Guid tenantId,
        Guid prizeAwardId,
        Guid prizeId,
        string prizeNameSnapshot,
        decimal prizeCostSnapshot,
        DateTime utcNow)
    {
        return new PrizeAwardOption(
            Guid.NewGuid(),
            tenantId,
            prizeAwardId,
            prizeId,
            prizeNameSnapshot,
            prizeCostSnapshot,
            utcNow);
    }
}
