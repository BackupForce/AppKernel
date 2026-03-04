namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetRedeemedWinningsByDateRangeRequest(
    DateTime? RedeemedFromUtc = null,
    DateTime? RedeemedToUtc = null,
    int Page = 1,
    int PageSize = 50);
