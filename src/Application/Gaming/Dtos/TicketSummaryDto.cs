using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

/// <summary>
/// 票券摘要資料，提供會員查詢。
/// </summary>
public sealed record TicketSummaryDto(
    Guid TicketId,
    Guid? CampaignId,
    string GameCode,
    string? PlayTypeCode,
    TicketSubmissionStatus SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    IReadOnlyCollection<TicketLineSummaryDto> Lines,
    IReadOnlyCollection<TicketDrawSummaryDto> Draws);
