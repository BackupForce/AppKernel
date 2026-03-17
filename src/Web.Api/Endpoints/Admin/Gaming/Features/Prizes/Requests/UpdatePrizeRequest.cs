namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

/// <summary>
/// 更新獎品的 API 請求資料。
/// </summary>
public sealed record UpdatePrizeRequest(
    string Name,
    string? Description,
    decimal Cost);
