using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Admin.OperationLogs;
using Domain.Security;
using Domain.Users;
using SharedKernel;

namespace Application.Users.RemoveRole;

internal sealed class RemoveUserRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IAuthzCacheInvalidator invalidator,
    IAdminOperationLogRepository adminOperationLogRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext)
    : ICommandHandler<RemoveUserRoleCommand, RemoveUserRoleResultDto>
{
    public async Task<Result<RemoveUserRoleResultDto>> Handle(
        RemoveUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<RemoveUserRoleResultDto>(UserErrors.NotFound(request.UserId));
        }

        Guid? resolvedTenantId = ResolveTenantId(userContext, tenantContext);
        if (resolvedTenantId.HasValue && (!user.TenantId.HasValue || user.TenantId.Value != resolvedTenantId.Value))
        {
            return Result.Failure<RemoveUserRoleResultDto>(UserErrors.NotFound(request.UserId));
        }

        Role? role = await roleRepository.GetByIdAsync(request.RoleId, false, cancellationToken);
        if (role is null)
        {
            return Result.Failure<RemoveUserRoleResultDto>(RoleErrors.NotFound);
        }

        if (!CanManageRole(user, role))
        {
            return Result.Failure<RemoveUserRoleResultDto>(UserErrors.NotFound(request.UserId));
        }

        if (RoleRestrictions.IsRemovalRestricted(role.Name))
        {
            return Result.Failure<RemoveUserRoleResultDto>(UserErrors.RoleRemovalNotAllowed(role.Name));
        }

        if (!user.HasRole(role.Id))
        {
            return Result.Success(BuildResult(user));
        }

        user.RemoveRole(role);

        Guid? logTenantId = user.TenantId ?? resolvedTenantId;
        if (logTenantId.HasValue)
        {
            DateTime now = dateTimeProvider.UtcNow;
            string metadataJson = JsonSerializer.Serialize(new
            {
                roleId = role.Id,
                roleName = role.Name
            });

            AdminOperationLog log = AdminOperationLog.Create(
                logTenantId.Value,
                "User",
                user.Id,
                "RemoveRole",
                userContext.UserId,
                operatorType: userContext.UserType.ToString(),
                reason: null,
                metadataJson,
                now);

            await adminOperationLogRepository.AddAsync(log, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await invalidator.InvalidateRoleAsync(role.Id, cancellationToken);
        await invalidator.UntrackRoleUserAsync(role.Id, user.Id, cancellationToken);
        await invalidator.InvalidateUserAsync(user.Id, cancellationToken);

        return Result.Success(BuildResult(user));
    }

    private static RemoveUserRoleResultDto BuildResult(User user)
    {
        List<int> roleIds = user.Roles.Select(role => role.Id).OrderBy(id => id).ToList();
        return new RemoveUserRoleResultDto(user.Id, roleIds);
    }

    private static bool CanManageRole(User user, Role role)
    {
        if (user.Type == UserType.Platform)
        {
            return role.IsPlatformRole();
        }

        if (user.Type == UserType.Tenant)
        {
            return user.TenantId.HasValue
                && role.TenantId.HasValue
                && role.TenantId.Value == user.TenantId.Value;
        }

        return false;
    }

    private static Guid? ResolveTenantId(IUserContext userContext, ITenantContext tenantContext)
    {
        if (userContext.TenantId.HasValue)
        {
            return userContext.TenantId.Value;
        }

        if (tenantContext.TryGetTenantId(out Guid tenantId))
        {
            return tenantId;
        }

        return null;
    }
}
