namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

/// <summary>
/// 建立獎品的 API 請求資料。
/// </summary>
public sealed record CreatePrizeRequest(
    string Name,
    string? Description,
    decimal Cost);
