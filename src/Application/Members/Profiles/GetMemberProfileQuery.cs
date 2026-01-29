using Application.Abstractions.Messaging;

namespace Application.Members.Profiles;

public sealed record GetMemberProfileQuery(Guid MemberId) : IQuery<MemberProfileDto>;
