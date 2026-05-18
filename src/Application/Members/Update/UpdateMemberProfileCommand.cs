using Application.Abstractions.Messaging;

namespace Application.Members.Update;

public sealed record UpdateMemberProfileCommand(Guid MemberId, string DisplayName) : ICommand;
