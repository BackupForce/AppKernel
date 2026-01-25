namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record GetAvailableTicketsForBetRequest(Guid? DrawId, int? Limit);
