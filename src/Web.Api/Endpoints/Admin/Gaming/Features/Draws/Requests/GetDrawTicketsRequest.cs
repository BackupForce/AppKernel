namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

public sealed record GetDrawTicketsRequest(
    int Page = 1,
    int PageSize = 20);
