namespace Web.Api.Endpoints.Frontends.Requests;

/// <summary>
/// 會員票券查詢的 API 參數。
/// </summary>
public sealed record GetMyTicketsRequest(
    string GameCode,
    DateTime? From,
    DateTime? To,
    int PageNumber = 1,
    int PageSize = 20);
