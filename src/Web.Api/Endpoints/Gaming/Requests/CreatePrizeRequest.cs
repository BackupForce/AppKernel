namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 建立獎品的 API 請求資料。
/// </summary>
public sealed record CreatePrizeRequest(
    string Name,
    string? Description,
    decimal Cost);
