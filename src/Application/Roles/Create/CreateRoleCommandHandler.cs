using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using SharedKernel;

namespace Application.Roles.Create;

internal sealed class CreateRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand, int>
{
    public async Task<Result<int>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<int>(RoleErrors.NameRequired);
        }

        string name = request.Name.Trim();

        bool isUnique = await roleRepository.IsNameUniqueAsync(name, null, cancellationToken);
        if (!isUnique)
        {
            return Result.Failure<int>(RoleErrors.NameConflict);
        }

        var role = Role.Create(name);

        roleRepository.Insert(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
