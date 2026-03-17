namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

public sealed record GetAdminWinningsByDrawRequest(
    string? Q = null,
    bool? Redeemed = null,
    int Current = 1,
    int PageSize = 50);
