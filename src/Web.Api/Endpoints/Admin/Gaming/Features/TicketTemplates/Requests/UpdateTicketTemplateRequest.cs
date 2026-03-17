using Domain.Gaming.TicketTemplates;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

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
