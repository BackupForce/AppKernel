namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetWinningCostReportRequest(
    DateTime FromUtc,
    DateTime ToUtc,
    string? GameCode = null,
    Guid? DrawGroupId = null,
    bool IncludeDetails = false,
    int Page = 1,
    int PageSize = 50);
