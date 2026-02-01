namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record RemoteSearchDrawGroupsRequest(
    string? Q,
    int Page = 1,
    int PageSize = 20);
