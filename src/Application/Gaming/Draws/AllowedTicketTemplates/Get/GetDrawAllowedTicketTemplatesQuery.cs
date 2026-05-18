using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.AllowedTicketTemplates.Get;

/// <summary>
/// 取得期數允許票種清單查詢。
/// </summary>
public sealed record GetDrawAllowedTicketTemplatesQuery(Guid DrawId)
    : IQuery<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>;
