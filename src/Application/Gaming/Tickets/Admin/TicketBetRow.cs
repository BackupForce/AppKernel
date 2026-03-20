using Domain.Gaming.Tickets;

namespace Application.Gaming.Tickets.Admin;

internal sealed record TicketBetRow(
    Guid TicketId,
    Guid MemberId,
    string MemberNo,
    string DisplayName,
    string GameCode,
    TicketSubmissionStatus SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    TicketDrawParticipationStatus ParticipationStatus,
    int? LineIndex,
    string? PlayTypeCode,
    string? Numbers);
