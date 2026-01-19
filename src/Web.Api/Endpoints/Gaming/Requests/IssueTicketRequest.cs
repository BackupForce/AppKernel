namespace Web.Api.Endpoints.Gaming.Requests;

public sealed record IssueTicketRequest(
    Guid MemberId,
    Guid CampaignId,
    Guid? TicketTemplateId,
    string? IssuedReason);
