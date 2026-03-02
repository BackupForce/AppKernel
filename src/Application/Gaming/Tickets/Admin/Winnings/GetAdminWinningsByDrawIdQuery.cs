using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin.Winnings;

public sealed record GetAdminWinningsByDrawIdQuery(
    Guid TenantId,
    Guid DrawId,
    string? Keyword,
    bool? Redeemed,
    int Current,
    int PageSize) : IQuery<PagedResult<AdminWinningListItemDto>>;
