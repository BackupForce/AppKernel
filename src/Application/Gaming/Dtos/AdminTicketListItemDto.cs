using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

/// <summary>
/// 後台查詢票券列表項目。
/// </summary>
public sealed record AdminTicketListItemDto(
    Guid TicketId,
    Guid MemberId,
    string? MemberNo,
    string GameCode,
    Guid? DrawId,
    TicketSubmissionStatus SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    DateTime? CancelledAtUtc,
    IssuedByType IssuedByType,
    Guid? IssuedByUserId,
    Guid? SubmittedByUserId,
    int LineCount,
    DateTime CreatedAt);
