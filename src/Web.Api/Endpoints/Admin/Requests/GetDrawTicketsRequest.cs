namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetDrawTicketsRequest(
    int Page = 1,
    int PageSize = 20);
