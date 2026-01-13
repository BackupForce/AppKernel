namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 兌換得獎的 API 請求資料。
/// </summary>
public sealed record RedeemPrizeAwardRequest(Guid PrizeId, string? Note);
