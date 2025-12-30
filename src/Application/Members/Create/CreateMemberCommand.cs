using Application.Abstractions.Messaging;

namespace Application.Members.Create;

public sealed record CreateMemberCommand(
    Guid? UserId,
    string DisplayName,
    string? MemberNo) : ICommand<Guid>;
