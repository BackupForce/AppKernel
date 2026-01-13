using Domain.Gaming;

namespace Web.Api.Endpoints.Gaming.Requests;

/// <summary>
/// 更新票種模板的 API 請求資料。
/// </summary>
public sealed record UpdateTicketTemplateRequest(
    string Code,
    string Name,
    TicketTemplateType Type,
    decimal Price,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    int MaxLinesPerTicket);
