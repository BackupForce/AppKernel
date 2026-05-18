using Domain.Members;
using SharedKernel;

namespace Domain.Members.Events;

public sealed record MemberStatusChangedDomainEvent(Guid MemberId, MemberStatus Status) : IDomainEvent;
