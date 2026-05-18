using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

/// <summary>
/// 後台查詢期數下注票券摘要。
/// </summary>
public sealed record DrawTicketBetDto(
    Guid TicketId,
    Guid MemberId,
    string MemberNo,
    string DisplayName,
    string GameCode,
    TicketSubmissionStatus SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    TicketDrawParticipationStatus ParticipationStatus,
    IReadOnlyCollection<TicketLineDetailDto> Lines);
