namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetWinningCostByDrawRequest(
    int DetailPage = 1,
    int DetailPageSize = 100);
