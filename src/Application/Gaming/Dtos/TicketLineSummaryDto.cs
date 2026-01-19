namespace Application.Gaming.Dtos;

/// <summary>
/// 票券注數摘要。
/// </summary>
public sealed record TicketLineSummaryDto(
    int LineIndex,
    string Numbers);
