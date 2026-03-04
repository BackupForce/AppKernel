using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin.Winnings;

public sealed record GetRedeemedWinningsByDateRangeQuery(
    Guid TenantId,
    DateTime? RedeemedFromUtc,
    DateTime? RedeemedToUtc,
    int Page,
    int PageSize)
    : IQuery<PagedResult<RedeemedWinningListItemDto>>;
