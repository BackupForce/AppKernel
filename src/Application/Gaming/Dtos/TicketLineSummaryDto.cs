namespace Application.Gaming.Dtos;

public sealed record TicketLineSummaryDto(
    int LineIndex,
    string Numbers,
    int MatchedCount);
