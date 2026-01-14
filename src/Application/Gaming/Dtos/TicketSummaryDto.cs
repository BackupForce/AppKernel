namespace Application.Gaming.Dtos;

/// <summary>
/// 票券摘要資料，提供會員查詢。
/// </summary>
public sealed record TicketSummaryDto(
    Guid TicketId,
    Guid DrawId,
    string GameCode,
    string PlayTypeCode,
    long TotalCost,
    DateTime CreatedAt,
    IReadOnlyCollection<TicketLineSummaryDto> Lines);
