namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record PlaceTicketRequest(IReadOnlyCollection<IReadOnlyCollection<int>> Lines);
