using Application.Abstractions.Messaging;

namespace Application.Members.Addresses;

public sealed record GetMemberAddressesQuery(Guid MemberId) : IQuery<IReadOnlyList<MemberAddressDto>>;
