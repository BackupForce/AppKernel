namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 建立期數的 API 請求資料。
/// </summary>
public sealed record CreateDrawRequest(
    DateTime SalesStartAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    int? RedeemValidDays);
