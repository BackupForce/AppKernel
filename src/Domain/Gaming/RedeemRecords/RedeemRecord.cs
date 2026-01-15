using SharedKernel;

namespace Domain.Gaming.RedeemRecords;

/// <summary>
/// 兌換紀錄，保存獎品兌換的成本快照與備註。
/// </summary>
/// <remarks>
/// CostSnapshot 是為了避免後續獎品成本變更影響歷史報表。
/// </remarks>
public sealed class RedeemRecord : Entity
{
    private RedeemRecord(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid prizeAwardId,
        Guid prizeId,
        string prizeNameSnapshot,
        decimal costSnapshot,
        DateTime redeemedAt,
        string? note) : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        PrizeAwardId = prizeAwardId;
        PrizeId = prizeId;
        PrizeNameSnapshot = prizeNameSnapshot;
        CostSnapshot = costSnapshot;
        RedeemedAt = redeemedAt;
        Note = note;
    }

    private RedeemRecord()
    {
    }

    /// <summary>
    /// 租戶識別。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 兌換會員識別。
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// 對應的得獎記錄識別。
    /// </summary>
    public Guid PrizeAwardId { get; private set; }

    /// <summary>
    /// 對應的獎品識別。
    /// </summary>
    public Guid PrizeId { get; private set; }

    /// <summary>
    /// 兌換時獎品名稱快照，避免後台改名影響歷史紀錄。
    /// </summary>
    public string PrizeNameSnapshot { get; private set; } = string.Empty;

    /// <summary>
    /// 兌換時成本快照，用於報表與成本核算。
    /// </summary>
    public decimal CostSnapshot { get; private set; }

    /// <summary>
    /// 兌換時間（UTC）。
    /// </summary>
    public DateTime RedeemedAt { get; private set; }

    /// <summary>
    /// 兌換備註。
    /// </summary>
    public string? Note { get; private set; }

    /// <summary>
    /// 建立兌換紀錄，成本需以當下成本快照寫入。
    /// </summary>
    public static RedeemRecord Create(
        Guid tenantId,
        Guid memberId,
        Guid prizeAwardId,
        Guid prizeId,
        string prizeNameSnapshot,
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
            prizeNameSnapshot,
            costSnapshot,
            redeemedAt,
            note);
    }
}
