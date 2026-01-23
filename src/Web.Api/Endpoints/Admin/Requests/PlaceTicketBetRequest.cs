namespace Web.Api.Endpoints.Admin.Requests;

public sealed record PlaceTicketBetRequest(
    IReadOnlyCollection<int> Numbers,
    string? ClientReference,
    string? Note);
