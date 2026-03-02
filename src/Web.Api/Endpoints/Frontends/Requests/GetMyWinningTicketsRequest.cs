namespace Web.Api.Endpoints.Frontends.Requests;

public sealed record GetMyWinningTicketsRequest(
    int PageNumber = 1,
    int PageSize = 20);
