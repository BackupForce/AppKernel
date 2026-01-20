namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record ListCampaignsRequest(
    string? Status,
    string? GameCode,
    string? Keyword,
    int Page = 1,
    int PageSize = 20);
