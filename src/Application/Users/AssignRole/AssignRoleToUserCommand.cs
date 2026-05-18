using Application.Abstractions.Messaging;

namespace Application.Users.AssignRole;

public sealed record AssignRoleToUserCommand(Guid UserId, int RoleId)
    : ICommand<AssignRoleToUserResultDto>;
