using Application.Abstractions.Messaging;
using Application.Members.Dtos;

namespace Application.Members.Assets.GetAssets;

public sealed record GetMemberAssetsQuery(Guid MemberId) : IQuery<IReadOnlyCollection<MemberAssetBalanceDto>>;
