namespace Web.Api.Endpoints.Frontends.Requests;

/// <summary>
/// 會員得獎查詢的 API 參數。
/// </summary>
public sealed record GetMyAwardsRequest(string GameCode, string? Status);
