namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 會員票券查詢的 API 參數。
/// </summary>
public sealed record GetMyTicketsRequest(DateTime? From, DateTime? To);
