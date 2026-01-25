namespace Web.Api.Endpoints.Admin.Requests;

public sealed record GetMemberAvailableTicketsForBetRequest(Guid? DrawId, int? Limit);
