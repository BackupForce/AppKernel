using Application.Abstractions.Caching;
using Application.Members.Dtos;

namespace Application.Members.GetById;

public sealed record GetMemberByIdQuery(Guid MemberId) : ICachedQuery<MemberDetailDto>
{
    public string CacheKey => $"members:id:{MemberId}";

    public TimeSpan? Expiration => TimeSpan.FromSeconds(60);
}
