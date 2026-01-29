namespace Domain.Gaming.Tickets;

/// <summary>
/// 票券參與期數（Draw）的狀態
/// </summary>
public enum TicketDrawParticipationStatus
{
    /// <summary>
    /// 待處理：
    /// 票券已建立，但尚未完成投注或尚未正式參與期數
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 有效參與中：
    /// 已完成投注，且期數尚未封盤或尚未開獎
    /// </summary>
    Active = 1,

    /// <summary>
    /// 無效：
    /// 票券未在封盤前完成投注，或因規則判定為無效
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// 已結算：
    /// 期數已開獎並完成派彩計算（不代表一定已兌獎）
    /// </summary>
    Settled = 3,

    /// <summary>
    /// 已兌獎：
    /// 中獎票券已完成兌獎流程
    /// </summary>
    Redeemed = 4,

    /// <summary>
    /// 已取消：
    /// 因系統、人工或活動調整等原因取消參與
    /// </summary>
    Cancelled = 5
}
