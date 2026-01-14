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
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        Guid ticketId,
        int lineIndex,
        int matchedCount,
        PrizeTier prizeTier,
        Guid prizeId,
        string prizeNameSnapshot,
        decimal prizeCostSnapshot,
        int? prizeRedeemValidDaysSnapshot,
        string? prizeDescriptionSnapshot,
        DateTime? expiresAt,
        AwardStatus status,
        DateTime awardedAt) : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        DrawId = drawId;
        GameCode = gameCode;
        PlayTypeCode = playTypeCode;
        TicketId = ticketId;
        LineIndex = lineIndex;
        MatchedCount = matchedCount;
        PrizeTier = prizeTier;
        PrizeId = prizeId;
        PrizeNameSnapshot = prizeNameSnapshot;
        PrizeCostSnapshot = prizeCostSnapshot;
        PrizeRedeemValidDaysSnapshot = prizeRedeemValidDaysSnapshot;
        PrizeDescriptionSnapshot = prizeDescriptionSnapshot;
        ExpiresAt = expiresAt;
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
    /// 遊戲代碼快照。
    /// </summary>
    public GameCode GameCode { get; private set; }

    /// <summary>
    /// 玩法代碼快照。
    /// </summary>
    public PlayTypeCode PlayTypeCode { get; private set; }

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
    /// 命中的獎級。
    /// </summary>
    public PrizeTier PrizeTier { get; private set; }

    /// <summary>
    /// 對應的獎品識別。
    /// </summary>
    public Guid PrizeId { get; private set; }

    /// <summary>
    /// 獎品名稱快照。
    /// </summary>
    public string PrizeNameSnapshot { get; private set; } = string.Empty;

    /// <summary>
    /// 獎品成本快照。
    /// </summary>
    public decimal PrizeCostSnapshot { get; private set; }

    /// <summary>
    /// 獎品兌獎有效天數快照。
    /// </summary>
    public int? PrizeRedeemValidDaysSnapshot { get; private set; }

    /// <summary>
    /// 獎品描述快照。
    /// </summary>
    public string? PrizeDescriptionSnapshot { get; private set; }

    /// <summary>
    /// 兌獎到期時間（UTC），過期後不得兌換。
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

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
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        Guid ticketId,
        int lineIndex,
        int matchedCount,
        PrizeTier prizeTier,
        PrizeOption prizeOption,
        DateTime? expiresAt,
        DateTime awardedAt)
    {
        Guid prizeId = prizeOption.PrizeId ?? Guid.Empty;

        return new PrizeAward(
            Guid.NewGuid(),
            tenantId,
            memberId,
            drawId,
            gameCode,
            playTypeCode,
            ticketId,
            lineIndex,
            matchedCount,
            prizeTier,
            prizeId,
            prizeOption.Name,
            prizeOption.Cost,
            prizeOption.RedeemValidDays,
            prizeOption.Description,
            expiresAt,
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

    /// <summary>
    /// 將獎項標記為過期。
    /// </summary>
    public void Expire(DateTime utcNow)
    {
        Status = AwardStatus.Expired;
        ExpiresAt = utcNow; 
    }
}
