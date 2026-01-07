using Application.Abstractions.Messaging;

namespace Application.Users.RemoveGroup;

public sealed record RemoveGroupFromUserCommand(Guid UserId, Guid GroupId)
    : ICommand<RemoveGroupFromUserResultDto>;
