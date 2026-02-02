namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record RemoteSearchDrawsRequest(
    string? Q,
    int Page = 1,
    int PageSize = 20);
