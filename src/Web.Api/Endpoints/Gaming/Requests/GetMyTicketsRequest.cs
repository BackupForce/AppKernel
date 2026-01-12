namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record GetMyTicketsRequest(DateTime? From, DateTime? To);
