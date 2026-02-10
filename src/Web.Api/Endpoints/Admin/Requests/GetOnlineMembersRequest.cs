namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetOnlineMembersRequest(
    int Page = 1,
    int PageSize = 20,
    int WindowMinutes = 5,
    string? Q = null);
