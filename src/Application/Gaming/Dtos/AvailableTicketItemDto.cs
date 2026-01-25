namespace Application.Gaming.Dtos;

public sealed record AvailableTicketItemDto(
    Guid TicketId,
    string DisplayText,
    string GameCode,
    Guid? DrawId,
    DateTime? SalesCloseAtUtc,
    DateTime? ExpiresAtUtc,
    IReadOnlyList<TicketPlayTypeDto> AvailablePlayTypes);
