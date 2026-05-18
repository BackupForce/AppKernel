using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin;

/// <summary>
/// 後台查詢指定期數的已成立票券列表。
/// </summary>
public sealed record GetDrawTicketsQuery(Guid DrawId, int Page, int PageSize)
    : IQuery<PagedResult<DrawTicketBetDto>>;
