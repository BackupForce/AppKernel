using Application.Abstractions.Messaging;
using Application.Abstractions.Data;
using Application.Members.Dtos;

namespace Application.Members.Tags.List;

public sealed record ListMemberTagsQuery(
    Guid TenantId,
    string? Keyword,
    bool? IsActive,
    int Page,
    int PageSize) : IQuery<PagedResult<MemberTagDto>>;
