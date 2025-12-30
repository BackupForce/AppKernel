using Application.Abstractions.Caching;
using Application.Members.Dtos;

namespace Application.Members.Points.GetBalance;

public sealed record GetMemberPointBalanceQuery(Guid MemberId) : ICachedQuery<MemberPointBalanceDto>
{
    public string CacheKey => $"members:points:balance:{MemberId}";

    public TimeSpan? Expiration => TimeSpan.FromSeconds(15);
}
