namespace Application.Gaming.Dtos;

public sealed record TicketSummaryDto(
    Guid TicketId,
    Guid DrawId,
    long TotalCost,
    DateTime CreatedAt,
    IReadOnlyCollection<TicketLineSummaryDto> Lines);
