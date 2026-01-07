using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class RemoveRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveRolePermissionsCommand>
{
    public async Task<Result> Handle(RemoveRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.RoleId, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string code in request.PermissionCodes)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                codes.Add(code.Trim());
            }
        }

        if (codes.Count == 0)
        {
            return Result.Failure(RoleErrors.PermissionCodesRequired);
        }

        await roleRepository.RemovePermissionsAsync(request.RoleId, codes, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.InvalidateRoleAsync(request.RoleId, cancellationToken);

        return Result.Success();
    }
}
