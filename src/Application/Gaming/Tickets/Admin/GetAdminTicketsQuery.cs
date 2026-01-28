using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Gaming.Tickets;

namespace Application.Gaming.Tickets.Admin;

/// <summary>
/// 後台查詢票券列表（支援條件篩選）。
/// </summary>
public sealed record GetAdminTicketsQuery(
    Guid TenantId,
    Guid? DrawId,
    TicketSubmissionStatus? SubmissionStatus,
    Guid? MemberId,
    string? MemberNo,
    DateTime? IssuedFromUtc,
    DateTime? IssuedToUtc,
    DateTime? SubmittedFromUtc,
    DateTime? SubmittedToUtc,
    DateTime? CreatedFromUtc,
    DateTime? CreatedToUtc,
    int Page,
    int PageSize)
    : IQuery<PagedResult<AdminTicketListItemDto>>;
