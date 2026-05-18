using Domain.Gaming.Tickets;

namespace Application.Gaming.Tickets.Submission;

internal sealed record TicketBetSubmissionResult(
    Guid TicketId,
    TicketSubmissionStatus SubmissionStatus,
    DateTime SubmittedAtUtc,
    Guid SubmittedByUserId,
    string PlayTypeCode,
    int[] Numbers,
    string? ClientReference,
    string? Note);
