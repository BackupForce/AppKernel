using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class AddRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddRolePermissionsCommand>
{
    public async Task<Result> Handle(AddRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, true, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (!RolePermissionUpdatePolicy.IsRoleAccessible(userContext, role))
        {
            // 中文註解：避免跨租戶或 Member 操作角色權限。
            return Result.Failure(RoleErrors.OperationNotAllowed);
        }

        HashSet<string> existingCodes = RolePermissionUpdatePolicy.NormalizeCodes(role.Permissions.Select(permission => permission.Name));
        HashSet<string> requestedCodes = RolePermissionUpdatePolicy.NormalizeCodes(request.PermissionCodes);

        Result validationResult = RolePermissionUpdatePolicy.ValidateRequestedCodes(role, requestedCodes, requireNonEmpty: true);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        List<Permission> permissionsToAdd = new List<Permission>();
        foreach (string code in requestedCodes)
        {
            if (!existingCodes.Contains(code))
            {
                Permission permission = Permission.CreateForRole(code, string.Empty, role.Id);
                permissionsToAdd.Add(permission);
            }
        }

        if (permissionsToAdd.Count == 0)
        {
            return Result.Success();
        }

        await roleRepository.AddPermissionsAsync(permissionsToAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.InvalidateRoleAsync(request.RoleId, cancellationToken);

        return Result.Success();
    }

}