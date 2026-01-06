using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Update;

internal sealed class UpdateRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(RoleErrors.NameRequired);
        }

        Role? role = await roleRepository.GetByIdAsync(request.Id, false, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        string name = request.Name.Trim();

        bool isUnique = await roleRepository.IsNameUniqueAsync(name, request.Id, cancellationToken);
        if (!isUnique)
        {
            return Result.Failure(RoleErrors.NameConflict);
        }

        role.Rename(name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
