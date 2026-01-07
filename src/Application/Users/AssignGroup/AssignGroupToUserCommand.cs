using Application.Abstractions.Messaging;

namespace Application.Users.AssignGroup;

public sealed record AssignGroupToUserCommand(Guid UserId, Guid GroupId)
    : ICommand<AssignGroupToUserResultDto>;
