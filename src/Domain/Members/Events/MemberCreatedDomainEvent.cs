using SharedKernel;

namespace Domain.Members.Events;

public sealed record MemberCreatedDomainEvent(Guid MemberId, string MemberNo) : IDomainEvent;
