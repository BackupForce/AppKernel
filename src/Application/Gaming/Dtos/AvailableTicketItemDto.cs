namespace Application.Gaming.Dtos;

public sealed record AvailableTicketItemDto(
    Guid TicketId,
    string DisplayText,
    string? GameCode,
    string? PlayTypeCode,
    Guid? DrawId,
    DateTime? SalesCloseAtUtc,
    DateTime? ExpiresAtUtc);
