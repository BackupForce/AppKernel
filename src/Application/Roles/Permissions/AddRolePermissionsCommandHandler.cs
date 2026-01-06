using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Permissions;

internal sealed class AddRolePermissionsCommandHandler(
    IRoleRepository roleRepository,
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

        var existingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Permission permission in role.Permissions)
        {
            existingCodes.Add(permission.Name);
        }

        var requestedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string code in request.PermissionCodes)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                requestedCodes.Add(code.Trim());
            }
        }

        if (requestedCodes.Count == 0)
        {
            return Result.Failure(RoleErrors.PermissionCodesRequired);
        }

        var permissionsToAdd = new List<Permission>();
        foreach (string code in requestedCodes)
        {
            if (!existingCodes.Contains(code))
            {
                var permission = Permission.CreateForRole(code, string.Empty, role.Id);
                permissionsToAdd.Add(permission);
            }
        }

        if (permissionsToAdd.Count == 0)
        {
            return Result.Success();
        }

        await roleRepository.AddPermissionsAsync(permissionsToAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
