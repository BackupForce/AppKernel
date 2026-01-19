using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Issue;

public sealed record IssueTicketCommand(
    Guid MemberId,
    Guid CampaignId,
    Guid? TicketTemplateId,
    string? IssuedReason) : ICommand<IssueTicketResult>;
