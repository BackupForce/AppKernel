using Domain.Gaming.TicketTemplates;

namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 建立票種模板的 API 請求資料。
/// </summary>
public sealed record CreateTicketTemplateRequest(
    string Code,
    string Name,
    TicketTemplateType Type,
    decimal Price,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int MaxLinesPerTicket);
