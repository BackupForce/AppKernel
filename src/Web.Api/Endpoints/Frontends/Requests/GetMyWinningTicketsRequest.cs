namespace Web.Api.Endpoints.Frontends.Requests;

public sealed record GetMyWinningTicketsRequest(
    int Page = 1,
    int PageSize = 20);
