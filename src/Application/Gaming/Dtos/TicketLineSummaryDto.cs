namespace Application.Gaming.Dtos;

/// <summary>
/// 票券注數摘要，包含命中數資訊。
/// </summary>
public sealed record TicketLineSummaryDto(
    int LineIndex,
    string Numbers,
    int MatchedCount);
