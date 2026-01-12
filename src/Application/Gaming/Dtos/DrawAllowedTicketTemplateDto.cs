namespace Application.Gaming.Dtos;

/// <summary>
/// 期數允許票種模板資料傳輸物件。
/// </summary>
public sealed record DrawAllowedTicketTemplateDto(
    Guid TicketTemplateId,
    string Code,
    string Name,
    string Type,
    decimal Price,
    bool IsActive);
