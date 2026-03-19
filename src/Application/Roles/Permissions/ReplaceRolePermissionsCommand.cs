using Application.Abstractions.Messaging;

namespace Application.Roles.Permissions;

public sealed record ReplaceRolePermissionsCommand(int RoleId, IReadOnlyCollection<string> PermissionCodes) : ICommand;
