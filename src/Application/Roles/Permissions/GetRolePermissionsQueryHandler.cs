using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class GetRolePermissionsQueryHandler(IRoleRepository roleRepository)
    : IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<string>>
{
    public async Task<Result<IReadOnlyList<string>>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure<IReadOnlyList<string>>(RoleErrors.NotFound);
        }

        IReadOnlyList<Permission> permissions = await roleRepository.GetPermissionsByRoleIdAsync(
            request.RoleId,
            cancellationToken);

        var permissionCodes = permissions
            .Select(permission => permission.Name)
            .OrderBy(code => code)
            .ToList();

        return permissionCodes;
    }
}
