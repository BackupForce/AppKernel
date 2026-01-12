namespace Application.Gaming.Dtos;

/// <summary>
/// 票種模板資料傳輸物件。
/// </summary>
public sealed record TicketTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string Type,
    decimal Price,
    bool IsActive,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int MaxLinesPerTicket,
    DateTime CreatedAt,
    DateTime UpdatedAt);
