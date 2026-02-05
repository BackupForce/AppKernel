namespace Application.Gaming.Tickets.GetMyWinningTickets;

public sealed record MyWinningTicketsDto(
    IReadOnlyList<MyWinningTicketItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record MyWinningTicketItemDto(
    Guid TicketId,
    string GameCode,
    int SubmissionStatus,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc,
    DateTime? ExpiresAtUtc,
    IReadOnlyList<MyWinningTicketDrawDto> Draws);

public sealed record MyWinningTicketDrawDto(
    Guid DrawId,
    string DrawCode,
    DateTime DrawAtUtc,
    int ParticipationStatus,
    string? WinningNumbers,
    string PrizeName,
    string? PrizeCode,
    decimal? PrizeAmount);
