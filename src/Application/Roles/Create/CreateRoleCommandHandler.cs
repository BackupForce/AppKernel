using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Roles.Create;

internal sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand, int>
{
    public async Task<Result<int>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<int>(RoleErrors.NameRequired);
        }

        string name = request.Name.Trim();
        Result<Guid?> tenantIdResult = ResolveTenantIdForRole(userContext);
        if (tenantIdResult.IsFailure)
        {
            return Result.Failure<int>(tenantIdResult.Error);
        }

        Guid? tenantId = tenantIdResult.Value;

        bool isUnique = await roleRepository.IsNameUniqueAsync(name, tenantId, null, cancellationToken);
        if (!isUnique)
        {
            return Result.Failure<int>(RoleErrors.NameConflict);
        }

        Role role = Role.Create(name, tenantId);

        roleRepository.Insert(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return role.Id;
    }

    private static Result<Guid?> ResolveTenantIdForRole(IUserContext userContext)
    {
        // 中文註解：依 UserType 分流建立角色，Member 一律拒絕以避免誤授權。
        if (userContext.UserType == UserType.Member)
        {
            return Result.Failure<Guid?>(RoleErrors.OperationNotAllowed);
        }

        if (userContext.UserType == UserType.Platform)
        {
            return Result.Success<Guid?>(null);
        }

        if (userContext.UserType == UserType.Tenant)
        {
            if (!userContext.TenantId.HasValue || userContext.TenantId.Value == Guid.Empty)
            {
                return Result.Failure<Guid?>(RoleErrors.OperationNotAllowed);
            }

            return Result.Success<Guid?>(userContext.TenantId.Value);
        }

        // 中文註解：未知狀態一律拒絕（Fail Closed）。
        return Result.Failure<Guid?>(RoleErrors.OperationNotAllowed);
    }
}
