using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin;

public sealed record GetWinningTicketsQuery(
    Guid TenantId,
    string? GameCode,
    Guid? DrawId,
    Guid? DrawGroupId,
    DateTime? DrawAtFromUtc,
    DateTime? DrawAtToUtc,
    RedemptionStatusFilter RedemptionStatus,
    string? Keyword,
    int Page,
    int PageSize)
    : IQuery<PagedResult<WinningTicketListItemDto>>;
