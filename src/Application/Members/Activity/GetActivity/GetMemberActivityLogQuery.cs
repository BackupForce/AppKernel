using Application.Abstractions.Messaging;
using Application.Abstractions.Data;
using Application.Members.Dtos;

namespace Application.Members.Activity.GetActivity;

public sealed record GetMemberActivityLogQuery(
    Guid MemberId,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Action,
    int Page,
    int PageSize) : IQuery<PagedResult<MemberActivityLogDto>>;
