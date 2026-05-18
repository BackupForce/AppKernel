using Application.Abstractions.Messaging;
using Application.Roles.Dtos;

namespace Application.Roles.List;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleListItemDto>>;
