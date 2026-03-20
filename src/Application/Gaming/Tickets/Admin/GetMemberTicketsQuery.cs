using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin;

public sealed record GetMemberTicketsQuery(
    Guid TenantId,
    Guid MemberId,
    int Page,
    int PageSize) : IQuery<PagedResult<DrawTicketBetDto>>;
