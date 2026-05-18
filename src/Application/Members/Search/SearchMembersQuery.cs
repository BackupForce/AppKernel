using Application.Abstractions.Messaging;
using Application.Abstractions.Data;
using Application.Members.Dtos;

namespace Application.Members.Search;

public sealed record SearchMembersQuery(
    string? Keyword,
    string? MemberNo,
    string? DisplayName,
    string? PhoneNumber,
    short? Status,
    Guid? UserId,
    int Page,
    int PageSize) : IQuery<PagedResult<MemberListItemDto>>;
