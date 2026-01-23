namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 取得可售票期數下拉選項的查詢參數。
/// </summary>
public sealed record GetSellingDrawOptionsRequest(
    string? GameCode,
    string? PlayTypeCode,
    int? Take);
