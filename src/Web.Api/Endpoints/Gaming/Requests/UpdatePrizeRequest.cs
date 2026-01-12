namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record UpdatePrizeRequest(
    string Name,
    string? Description,
    decimal Cost);
