namespace Application.Gaming.Dtos;

public sealed record AvailableTicketItemDto(
    Guid TicketId,
    string DisplayText,
    string ScopeDisplayName,
    string GameCode,
    Guid? DrawId,
    Guid? DrawGroupId,
    DateTime? SalesCloseAtUtc,
    DateTime ExpiresAtUtc,
    string? DrawTemplateName,
    IReadOnlyList<TicketPlayTypeDto> AvailablePlayTypes);
