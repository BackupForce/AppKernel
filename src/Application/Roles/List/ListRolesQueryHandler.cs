using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Roles.Dtos;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.List;

internal sealed class ListRolesQueryHandler(IRoleRepository roleRepository, IUserContext userContext)
    : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    public async Task<Result<IReadOnlyList<RoleListItemDto>>> Handle(
        ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Role> roles = await ResolveRolesAsync(
            roleRepository,
            userContext,
            cancellationToken);

        List<RoleListItemDto> items = roles
            .Select(role => new RoleListItemDto(role.Id, role.Name, role.Permissions.Count))
            .OrderBy(item => item.Id)
            .ToList();

        return items;
    }

    private static async Task<IReadOnlyList<Role>> ResolveRolesAsync(
        IRoleRepository roleRepository,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        if (userContext.UserType == UserType.Platform)
        {
            return await roleRepository.GetPlatformRolesAsync(cancellationToken);
        }

        if (userContext.UserType == UserType.Tenant)
        {
            if (!userContext.TenantId.HasValue || userContext.TenantId.Value == Guid.Empty)
            {
                return Array.Empty<Role>();
            }

            return await roleRepository.GetTenantRolesAsync(userContext.TenantId.Value, cancellationToken);
        }

        // 中文註解：Member 不使用角色清單，Fail Closed 直接回空。
        return Array.Empty<Role>();
    }
}
