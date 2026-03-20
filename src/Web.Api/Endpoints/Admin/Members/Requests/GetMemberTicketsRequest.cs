namespace Web.Api.Endpoints.Admin.Members.Requests;

public sealed record GetMemberTicketsRequest(
    int Page = 1,
    int PageSize = 20);
