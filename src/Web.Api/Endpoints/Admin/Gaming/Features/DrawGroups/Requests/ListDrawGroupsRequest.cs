namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;

public sealed record ListDrawGroupsRequest(
    string? Status,
    string? GameCode,
    string? Keyword,
    int Page = 1,
    int PageSize = 20);
