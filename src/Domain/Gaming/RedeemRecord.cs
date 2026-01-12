using SharedKernel;

namespace Domain.Gaming;

public sealed class RedeemRecord : Entity
{
    private RedeemRecord(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid prizeAwardId,
        Guid prizeId,
        decimal costSnapshot,
        DateTime redeemedAt,
        string? note) : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        PrizeAwardId = prizeAwardId;
        PrizeId = prizeId;
        CostSnapshot = costSnapshot;
        RedeemedAt = redeemedAt;
        Note = note;
    }

    private RedeemRecord()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid MemberId { get; private set; }

    public Guid PrizeAwardId { get; private set; }

    public Guid PrizeId { get; private set; }

    // 中文註解：兌換時成本快照，避免後續獎品成本變更影響報表。
    public decimal CostSnapshot { get; private set; }

    public DateTime RedeemedAt { get; private set; }

    public string? Note { get; private set; }

    public static RedeemRecord Create(
        Guid tenantId,
        Guid memberId,
        Guid prizeAwardId,
        Guid prizeId,
        decimal costSnapshot,
        DateTime redeemedAt,
        string? note)
    {
        return new RedeemRecord(
            Guid.NewGuid(),
            tenantId,
            memberId,
            prizeAwardId,
            prizeId,
            costSnapshot,
            redeemedAt,
            note);
    }
}
