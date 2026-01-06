using Application.Abstractions.Messaging;

namespace Application.Roles.Permissions;

public sealed record GetRolePermissionsQuery(int RoleId) : IQuery<IReadOnlyList<string>>;
