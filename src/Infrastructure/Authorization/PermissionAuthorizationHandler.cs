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
        (Guid? nodeId, Guid? tenantId) = await ResolveAuthorizationContextAsync(context);

        if (await _permissionProvider.HasPermissionAsync(userId, requiredPermission, nodeId, tenantId))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<(Guid? nodeId, Guid? tenantId)> ResolveAuthorizationContextAsync(
        AuthorizationHandlerContext context)
    {
        HttpContext? httpContext = context.Resource as HttpContext ?? _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return (null, null);
        }

        Guid? tenantId = TryResolveTenantId(httpContext);

        if (httpContext.Request.RouteValues.TryGetValue("id", out object? idValue) && TryGetGuid(idValue, out Guid id))
        {
            return (id, tenantId);
        }

        if (httpContext.Request.RouteValues.TryGetValue("externalKey", out object? externalKeyValue))
        {
            if (TryGetGuid(externalKeyValue, out Guid externalKeyGuid))
            {
                return (externalKeyGuid, tenantId);
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
                    return (nodeId, tenantId);
                }
            }
        }

        if (tenantId.HasValue)
        {
            Guid tenantNodeId = await ResolveTenantNodeIdAsync(tenantId.Value);
            if (tenantNodeId != Guid.Empty)
            {
                return (tenantNodeId, tenantId);
            }
        }

        return (null, tenantId);
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

    private static Guid? TryResolveTenantId(HttpContext httpContext)
    {
        if (httpContext.Request.RouteValues.TryGetValue("tenantId", out object? tenantValue) && TryGetGuid(tenantValue, out Guid tenantId))
        {
            return tenantId;
        }

        if (httpContext.Items.TryGetValue("TenantId", out object? tenantItem) && TryGetGuid(tenantItem, out Guid ttenantId))
        {
            return ttenantId;
        }

        return null;
    }

    private async Task<Guid> ResolveTenantNodeIdAsync(Guid tenantId)
    {
        Guid nodeId = await _dbContext.ResourceNodes
            .AsNoTracking()
            .Where(node => node.Id == tenantId)
            .Select(node => node.Id)
            .SingleOrDefaultAsync();

        if (nodeId != Guid.Empty)
        {
            return nodeId;
        }

        return await _dbContext.ResourceNodes
            .AsNoTracking()
            .Where(node => node.ExternalKey == tenantId.ToString("D"))
            .Select(node => node.Id)
            .SingleOrDefaultAsync();
    }
}
