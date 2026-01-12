namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record CreatePrizeRequest(
    string Name,
    string? Description,
    decimal Cost);
