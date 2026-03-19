using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class ReplaceRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ReplaceRolePermissionsCommand>
{
    public async Task<Result> Handle(ReplaceRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, true, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        if (!RolePermissionUpdatePolicy.IsRoleAccessible(userContext, role))
        {
            return Result.Failure(RoleErrors.OperationNotAllowed);
        }

        HashSet<string> requestedCodes = RolePermissionUpdatePolicy.NormalizeCodes(request.PermissionCodes);
        Result validationResult = RolePermissionUpdatePolicy.ValidateRequestedCodes(role, requestedCodes, requireNonEmpty: false);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        HashSet<string> existingCodes = RolePermissionUpdatePolicy.NormalizeCodes(role.Permissions.Select(permission => permission.Name));

        List<Permission> permissionsToAdd = requestedCodes
            .Where(code => !existingCodes.Contains(code))
            .Select(code => Permission.CreateForRole(code, string.Empty, role.Id))
            .ToList();

        List<string> permissionCodesToRemove = existingCodes
            .Where(code => !requestedCodes.Contains(code))
            .ToList();

        if (permissionsToAdd.Count == 0 && permissionCodesToRemove.Count == 0)
        {
            return Result.Success();
        }

        if (permissionCodesToRemove.Count > 0)
        {
            await roleRepository.RemovePermissionsAsync(role.Id, permissionCodesToRemove, cancellationToken);
        }

        if (permissionsToAdd.Count > 0)
        {
            await roleRepository.AddPermissionsAsync(permissionsToAdd, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.InvalidateRoleAsync(request.RoleId, cancellationToken);

        return Result.Success();
    }
}
