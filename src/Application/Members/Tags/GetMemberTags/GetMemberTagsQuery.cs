using Application.Abstractions.Messaging;
using Application.Members.Dtos;

namespace Application.Members.Tags.GetMemberTags;

public sealed record GetMemberTagsQuery(
    Guid TenantId,
    Guid MemberId) : IQuery<IReadOnlyCollection<MemberTagDto>>;
