using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.TicketTemplates.GetList;

/// <summary>
/// 取得票種模板清單查詢。
/// </summary>
public sealed record GetTicketTemplatesQuery(bool ActiveOnly) : IQuery<IReadOnlyCollection<TicketTemplateDto>>;
