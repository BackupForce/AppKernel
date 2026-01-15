namespace Domain.Gaming.PrizeAwards;

/// <summary>
/// 得獎狀態，用於兌換流程與防重判斷。
/// </summary>
public enum AwardStatus
{
    /// <summary>
    /// 已得獎但尚未兌換。
    /// </summary>
    Awarded = 0,
    /// <summary>
    /// 已兌換。
    /// </summary>
    Redeemed = 1,
    /// <summary>
    /// 已過期。
    /// </summary>
    Expired = 2,
    /// <summary>
    /// 已取消。
    /// </summary>
    Cancelled = 3
}
