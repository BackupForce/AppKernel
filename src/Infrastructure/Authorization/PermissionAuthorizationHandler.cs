using Application.Abstractions.Identity;
using Domain.Users;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using static Dapper.SqlMapper;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is not { Identity.IsAuthenticated: true })
        {
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();
        IRootUserService rootUserService = scope.ServiceProvider.GetRequiredService<IRootUserService>();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        Guid userId = context.User.GetUserId();

        // ⬇️ 查詢使用者資訊
        User? user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        // ⬇️ 如果是 root，直接通過
        if (rootUserService.IsRoot(user))
        {
            context.Succeed(requirement);
            return;
        }

        // ⬇️ 一般權限檢查
        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);
        string requiredPermission = requirement.Permission;

        if (permissions.Contains(requiredPermission))
        {
            context.Succeed(requirement);
            return;
        }

        // ⬇️ 加上總權限 fallback，例如 "users:*"
        string resourcePrefix = requiredPermission.Split(':')[0];
        string wildcardPermission = $"{resourcePrefix}:*";

        if (permissions.Contains(wildcardPermission))
        {
            context.Succeed(requirement);
        }
    }
}
