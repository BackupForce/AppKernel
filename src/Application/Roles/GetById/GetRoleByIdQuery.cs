using Application.Abstractions.Messaging;
using Application.Roles.Dtos;

namespace Application.Roles.GetById;

public sealed record GetRoleByIdQuery(int Id) : IQuery<RoleDetailDto>;
