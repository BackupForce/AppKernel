namespace Web.Api.Endpoints.Frontends.Requests;

public sealed record GetAvailableTicketsForBetRequest(Guid? DrawId, int? Limit);
