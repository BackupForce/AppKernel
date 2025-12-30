using Application.Abstractions.Messaging;

namespace Application.Members.Activate;

public sealed record ActivateMemberCommand(Guid MemberId, string? Reason) : ICommand;
