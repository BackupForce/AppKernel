namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetWinningTicketsRequest(
    string? GameCode = null,
    Guid? DrawId = null,
    Guid? DrawGroupId = null,
    DateTime? DrawAtFromUtc = null,
    DateTime? DrawAtToUtc = null,
    string? RedemptionStatus = null,
    string? Keyword = null,
    int Page = 1,
    int PageSize = 50);
