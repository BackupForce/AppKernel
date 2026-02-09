using Application.Abstractions.Messaging;

namespace Application.Users.RemoveRole;

public sealed record RemoveUserRoleCommand(Guid UserId, string RoleName)
    : ICommand<RemoveUserRoleResultDto>;
