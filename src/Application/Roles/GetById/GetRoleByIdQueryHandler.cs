using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Roles.Dtos;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.GetById;

internal sealed class GetRoleByIdQueryHandler(IRoleRepository roleRepository, IUserContext userContext)
    : IQueryHandler<GetRoleByIdQuery, RoleDetailDto>
{
    public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.Id, true, cancellationToken);

        if (role is null)
        {
            return Result.Failure<RoleDetailDto>(RoleErrors.NotFound);
        }

        if (!IsRoleAccessible(userContext, role))
        {
            // 中文註解：跨租戶或 Member 一律拒絕，避免資訊外洩。
            return Result.Failure<RoleDetailDto>(RoleErrors.OperationNotAllowed);
        }

        List<string> permissionCodes = role.Permissions
            .Select(permission => permission.Name)
            .OrderBy(code => code)
            .ToList();

        RoleDetailDto dto = new RoleDetailDto(role.Id, role.Name, permissionCodes);

        return dto;
    }

    private static bool IsRoleAccessible(IUserContext userContext, Role role)
    {
        if (userContext.UserType == UserType.Platform)
        {
            return role.IsPlatformRole();
        }

        if (userContext.UserType == UserType.Tenant)
        {
            return userContext.TenantId.HasValue
                && role.TenantId == userContext.TenantId.Value;
        }

        // 中文註解：Member 不允許操作角色。
        return false;
    }
}
