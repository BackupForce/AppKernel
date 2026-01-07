using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Delete;

internal sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository,
    IAuthzCacheInvalidator invalidator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        Role? role = await roleRepository.GetByIdAsync(request.Id, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        await roleRepository.RemovePermissionsByRoleIdAsync(role.Id, cancellationToken);

        roleRepository.Remove(role);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await invalidator.InvalidateRoleAsync(role.Id, cancellationToken);
        await invalidator.RemoveRoleIndexAsync(role.Id, cancellationToken);

        return Result.Success();
    }
}
