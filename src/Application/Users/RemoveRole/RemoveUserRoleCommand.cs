using Application.Abstractions.Messaging;

namespace Application.Users.RemoveRole;

public sealed record RemoveUserRoleCommand(Guid UserId, int RoleId)
    : ICommand<RemoveUserRoleResultDto>;
