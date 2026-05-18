namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;

public sealed record RemoteSearchDrawGroupsRequest(
    string? Q,
    int Page = 1,
    int PageSize = 20);
