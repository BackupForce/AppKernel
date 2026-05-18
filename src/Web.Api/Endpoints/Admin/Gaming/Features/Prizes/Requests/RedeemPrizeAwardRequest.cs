namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

/// <summary>
/// 兌換得獎的 API 請求資料。
/// </summary>
public sealed record RedeemPrizeAwardRequest(string? Note);
