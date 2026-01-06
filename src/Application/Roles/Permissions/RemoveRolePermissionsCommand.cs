using Application.Abstractions.Messaging;

namespace Application.Roles.Permissions;

public sealed record RemoveRolePermissionsCommand(int RoleId, IReadOnlyCollection<string> PermissionCodes) : ICommand;
