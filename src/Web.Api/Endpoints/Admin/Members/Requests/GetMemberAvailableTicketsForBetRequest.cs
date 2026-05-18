namespace Web.Api.Endpoints.Admin.Members.Requests;

public sealed record GetMemberAvailableTicketsForBetRequest(Guid? DrawId, int? Limit);
