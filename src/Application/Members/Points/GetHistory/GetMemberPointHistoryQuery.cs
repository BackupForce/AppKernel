using Application.Abstractions.Messaging;
using Application.Abstractions.Data;
using Application.Members.Dtos;

namespace Application.Members.Points.GetHistory;

public sealed record GetMemberPointHistoryQuery(
    Guid MemberId,
    DateTime? StartDate,
    DateTime? EndDate,
    short? Type,
    string? ReferenceType,
    string? ReferenceId,
    int Page,
    int PageSize) : IQuery<PagedResult<MemberPointLedgerDto>>;
