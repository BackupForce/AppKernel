namespace Web.Api.Endpoints.Admin.Requests;

public sealed record PlaceTicketBetRequest(
    string PlayTypeCode,
    IReadOnlyCollection<int> Numbers,
    string? ClientReference,
    string? Note);
