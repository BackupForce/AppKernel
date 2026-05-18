namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

/// <summary>
/// 建立期數的 API 請求資料。
/// </summary>
public sealed record CreateDrawRequest(
    Guid TemplateId,
    DateTime SalesStartAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    int? RedeemValidDays);
