using Application.Abstractions.Messaging;
using Application.Roles.Dtos;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.GetById;

internal sealed class GetRoleByIdQueryHandler(IRoleRepository roleRepository)
    : IQueryHandler<GetRoleByIdQuery, RoleDetailDto>
{
    public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.Id, true, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleDetailDto>(RoleErrors.NotFound);
        }

        var permissionCodes = role.Permissions
            .Select(permission => permission.Name)
            .OrderBy(code => code)
            .ToList();

        var dto = new RoleDetailDto(role.Id, role.Name, permissionCodes);

        return dto;
    }
}
