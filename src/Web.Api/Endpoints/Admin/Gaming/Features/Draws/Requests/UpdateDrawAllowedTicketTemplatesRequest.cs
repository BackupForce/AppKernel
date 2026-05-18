namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

/// <summary>
/// 更新期數允許票種清單請求。
/// </summary>
public sealed record UpdateDrawAllowedTicketTemplatesRequest(IReadOnlyCollection<Guid> TemplateIds);
