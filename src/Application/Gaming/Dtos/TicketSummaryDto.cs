using System.Text.Json.Serialization;
using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

/// <summary>
/// 票券摘要資料，提供會員查詢。
/// </summary>
public sealed partial record TicketSummaryDto(
    Guid TicketId,
    Guid? DrawGroupId,
    string DrawCode,
    string GameCode,
    string? PlayTypeCode,
    TicketSubmissionStatus SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    DateTime? ExpiresAtUtc,
    string? ClaimEventName,
    IReadOnlyCollection<TicketLineSummaryDto> Lines,
    IReadOnlyCollection<TicketDrawSummaryDto> Draws);
