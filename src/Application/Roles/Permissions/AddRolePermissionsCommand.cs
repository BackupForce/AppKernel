using Application.Abstractions.Messaging;

namespace Application.Roles.Permissions;

public sealed record AddRolePermissionsCommand(int RoleId, IReadOnlyCollection<string> PermissionCodes) : ICommand;
