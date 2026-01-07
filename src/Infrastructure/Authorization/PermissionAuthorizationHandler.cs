using Application.Abstractions.Authorization;
using Infrastructure.Authentication;
using Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

// 基於 permission provider 的授權處理器，支援資源節點授權判斷
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionProvider _permissionProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(
        IPermissionProvider permissionProvider,
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _permissionProvider = permissionProvider;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is null || context.User.Identity is null)
        {
            return;
        }

        if (!context.User.Identity.IsAuthenticated)
        {
            return;
        }

        Guid userId = context.User.GetUserId();
        string requiredPermission = requirement.PermissionCode;
        Guid? nodeId = await ResolveNodeIdAsync(context);

        if (await _permissionProvider.HasPermissionAsync(userId, requiredPermission, nodeId))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<Guid?> ResolveNodeIdAsync(AuthorizationHandlerContext context)
    {
        HttpContext? httpContext = context.Resource as HttpContext ?? _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.RouteValues.TryGetValue("id", out object? idValue))
        {
            if (TryGetGuid(idValue, out Guid id))
            {
                return id;
            }
        }

        if (httpContext.Request.RouteValues.TryGetValue("externalKey", out object? externalKeyValue))
        {
            if (TryGetGuid(externalKeyValue, out Guid externalKeyGuid))
            {
                return externalKeyGuid;
            }

            string? externalKey = externalKeyValue?.ToString();
            if (!string.IsNullOrWhiteSpace(externalKey))
            {
                Guid nodeId = await _dbContext.ResourceNodes
                    .AsNoTracking()
                    .Where(node => node.ExternalKey == externalKey)
                    .Select(node => node.Id)
                    .SingleOrDefaultAsync();

                if (nodeId != Guid.Empty)
                {
                    return nodeId;
                }
            }
        }

        return null;
    }

    private static bool TryGetGuid(object? value, out Guid id)
    {
        if (value is Guid guidValue)
        {
            id = guidValue;
            return true;
        }

        if (value is string stringValue && Guid.TryParse(stringValue, out Guid parsedId))
        {
            id = parsedId;
            return true;
        }

        id = Guid.Empty;
        return false;
    }
}
