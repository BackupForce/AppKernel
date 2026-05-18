using Application.Abstractions.Messaging;
using Application.Gaming.Tickets.Issue;

namespace Application.Gaming.Tickets.Claim;

public sealed record ClaimDrawGroupTicketCommand(Guid DrawGroupId) : ICommand<IssueTicketResult>;
