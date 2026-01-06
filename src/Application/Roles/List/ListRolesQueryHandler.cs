using Application.Abstractions.Messaging;
using Application.Roles.Dtos;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.List;

internal sealed class ListRolesQueryHandler(IRoleRepository roleRepository)
    : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    public async Task<Result<IReadOnlyList<RoleListItemDto>>> Handle(
        ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Role> roles = await roleRepository.ListAsync(cancellationToken);

        var items = roles
            .Select(role => new RoleListItemDto(role.Id, role.Name, role.Permissions.Count))
            .OrderBy(item => item.Id)
            .ToList();

        return items;
    }
}
