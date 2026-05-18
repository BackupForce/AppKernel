using Application.Abstractions.Messaging;

namespace Application.Members.Suspend;

public sealed record SuspendMemberCommand(Guid MemberId, string? Reason) : ICommand;
