namespace Application.Gaming.Tickets.Issue;

public sealed record IssueTicketResult(Guid TicketId, IReadOnlyCollection<Guid> DrawIds);
