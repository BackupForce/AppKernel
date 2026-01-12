namespace Domain.Gaming;

/// <summary>
/// 期數狀態，反映銷售與開獎流程的階段。
/// </summary>
public enum DrawStatus
{
    /// <summary>
    /// 已排程但尚未開放售票。
    /// </summary>
    Scheduled = 0,
    /// <summary>
    /// 售票開放中，可接受下注。
    /// </summary>
    SalesOpen = 1,
    /// <summary>
    /// 售票已截止，等待開獎。
    /// </summary>
    SalesClosed = 2,
    /// <summary>
    /// 已開獎並寫入中獎號碼與 proof。
    /// </summary>
    Settled = 3,
    /// <summary>
    /// 期數取消，通常是異常或營運決策。
    /// </summary>
    Cancelled = 4
}
