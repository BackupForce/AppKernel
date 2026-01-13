using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 中獎記錄，描述某注票券命中規則後的得獎結果。
/// </summary>
/// <remarks>
/// Award 狀態由結算與兌換流程驅動，且必須與會員身分綁定。
/// </remarks>
public sealed class PrizeAward : Entity
{
    private PrizeAward(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid drawId,
        Guid ticketId,
        int lineIndex,
        int matchedCount,
        Guid prizeId,
        AwardStatus status,
        DateTime awardedAt) : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        DrawId = drawId;
        TicketId = ticketId;
        LineIndex = lineIndex;
        MatchedCount = matchedCount;
        PrizeId = prizeId;
        Status = status;
        AwardedAt = awardedAt;
    }

    private PrizeAward()
    {
    }

    /// <summary>
    /// 租戶識別。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 得獎會員識別。
    /// </summary>
    public Guid MemberId { get; private set; }

    /// <summary>
    /// 期數識別。
    /// </summary>
    public Guid DrawId { get; private set; }

    /// <summary>
    /// 票券識別。
    /// </summary>
    public Guid TicketId { get; private set; }

    /// <summary>
    /// 票券內注數序號，用於對應結算與防重。
    /// </summary>
    public int LineIndex { get; private set; }

    /// <summary>
    /// 命中數。
    /// </summary>
    public int MatchedCount { get; private set; }

    /// <summary>
    /// 對應的獎品識別。
    /// </summary>
    public Guid PrizeId { get; private set; }

    /// <summary>
    /// 得獎狀態，從 Awarded 轉到 Redeemed/Expired/Cancelled。
    /// </summary>
    public AwardStatus Status { get; private set; }

    /// <summary>
    /// 中獎時間（UTC）。
    /// </summary>
    public DateTime AwardedAt { get; private set; }

    /// <summary>
    /// 兌換時間（UTC），僅在狀態為 Redeemed 時有值。
    /// </summary>
    public DateTime? RedeemedAt { get; private set; }

    /// <summary>
    /// 建立得獎記錄，初始狀態為 Awarded。
    /// </summary>
    public static PrizeAward Create(
        Guid tenantId,
        Guid memberId,
        Guid drawId,
        Guid ticketId,
        int lineIndex,
        int matchedCount,
        Guid prizeId,
        DateTime awardedAt)
    {
        return new PrizeAward(
            Guid.NewGuid(),
            tenantId,
            memberId,
            drawId,
            ticketId,
            lineIndex,
            matchedCount,
            prizeId,
            AwardStatus.Awarded,
            awardedAt);
    }

    /// <summary>
    /// 將得獎狀態改為已兌換。
    /// </summary>
    public void Redeem(DateTime utcNow)
    {
        Status = AwardStatus.Redeemed;
        RedeemedAt = utcNow;
    }
}
