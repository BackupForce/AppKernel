using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Infrastructure.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Domain.Security;
using Domain.Users;
using AppPermissionRequirement = Application.Abstractions.Authorization.PermissionRequirement;

namespace Infrastructure.Authorization;

// 基於 permission provider 的授權處理器，支援 tenant 資源節點授權判斷
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionProvider _permissionProvider;
    private readonly IPermissionEvaluator _permissionEvaluator;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(
        IPermissionProvider permissionProvider,
        IPermissionEvaluator permissionEvaluator,
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _permissionProvider = permissionProvider;
        _permissionEvaluator = permissionEvaluator;
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

        if (!JwtUserContext.TryFromClaims(context.User, out JwtUserContext? jwtContext) || jwtContext is null)
        {
            return;
        }

        // 中文註解：Member 使用者不走 PermissionProvider，避免誤授權。
        if (jwtContext.UserType == UserType.Member)
        {
            return;
        }

        string requiredPermission = requirement.PermissionCode;
        if (!PermissionCatalog.TryGetScope(requiredPermission, out PermissionScope scope))
        {
            // 中文註解：無法解析 scope 的權限碼一律拒絕（Fail Closed）。
            return;
        }

        (Guid? nodeId, Guid? routeTenantId) = await ResolveAuthorizationContextAsync(context);

        if (jwtContext.UserType == UserType.Tenant)
        {
            if (!jwtContext.TenantId.HasValue)
            {
                return;
            }

            if (routeTenantId.HasValue && routeTenantId.Value != jwtContext.TenantId.Value)
            {
                return;
            }
        }

        switch (scope)
        {
            case PermissionScope.Platform:
                if (jwtContext.UserType != UserType.Platform)
                {
                    return;
                }

                AppPermissionRequirement platformRequirement = new AppPermissionRequirement(
                    requiredPermission,
                    PermissionScope.Platform,
                    null,
                    null);
                if (await _permissionEvaluator.AuthorizeAsync(
                    platformRequirement,
                    new CallerContext(jwtContext.UserId),
                    CancellationToken.None))
                {
                    context.Succeed(requirement);
                }

                break;
            case PermissionScope.Tenant:
                if (jwtContext.UserType != UserType.Tenant)
                {
                    return;
                }

                Guid? tenantId = routeTenantId ?? jwtContext.TenantId;
                if (!tenantId.HasValue)
                {
                    return;
                }

                if (await _permissionProvider.HasPermissionAsync(jwtContext.UserId, requiredPermission, nodeId, tenantId))
                {
                    context.Succeed(requirement);
                }

                break;
            case PermissionScope.Self:
                // 中文註解：Self scope 需要目標使用者資訊，缺失時拒絕。
                return;
            default:
                return;
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
        Guid? nodeId = await ResolveNodeIdFromMetadataAsync(httpContext, tenantId);
        if (nodeId.HasValue)
        {
            return (nodeId, tenantId);
        }

        if (httpContext.Request.RouteValues.TryGetValue("nodeId", out object? nodeValue)
            && TryGetGuid(nodeValue, out Guid explicitNodeId))
        {
            return (explicitNodeId, tenantId);
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

    private async Task<Guid?> ResolveNodeIdFromMetadataAsync(HttpContext httpContext, Guid? tenantId)
    {
        if (!tenantId.HasValue)
        {
            return null;
        }

        ResourceNodeMetadata? metadata = httpContext.GetEndpoint()
            ?.Metadata
            .GetMetadata<ResourceNodeMetadata>();
        if (metadata is null)
        {
            return null;
        }

        if (!httpContext.Request.RouteValues.TryGetValue(metadata.RouteValueKey, out object? rawValue))
        {
            return null;
        }

        string? externalValue = rawValue switch
        {
            null => null,
            Guid guidValue => guidValue.ToString("D"),
            string stringValue => stringValue,
            _ => rawValue.ToString()
        };

        if (string.IsNullOrWhiteSpace(externalValue))
        {
            return null;
        }

        string externalKey = $"{metadata.ExternalKeyPrefix}{externalValue}";
        Guid nodeId = await _dbContext.ResourceNodes
            .AsNoTracking()
            .Where(node => node.TenantId == tenantId.Value && node.ExternalKey == externalKey)
            .Select(node => node.Id)
            .SingleOrDefaultAsync();

        return nodeId == Guid.Empty ? null : nodeId;
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
            .Where(node => node.TenantId == tenantId && node.ParentId == null)
            .Select(node => node.Id)
            .SingleOrDefaultAsync();

        if (nodeId != Guid.Empty)
        {
            return nodeId;
        }

        string? tenantCode = await _dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Id == tenantId)
            .Select(tenant => tenant.Code)
            .SingleOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(tenantCode))
        {
            nodeId = await _dbContext.ResourceNodes
                .AsNoTracking()
                .Where(node => node.TenantId == tenantId && node.ExternalKey == tenantCode)
                .Select(node => node.Id)
                .SingleOrDefaultAsync();

            if (nodeId != Guid.Empty)
            {
                return nodeId;
            }
        }

        return await _dbContext.ResourceNodes
            .AsNoTracking()
            .Where(node => node.TenantId == tenantId && node.ExternalKey == tenantId.ToString("D"))
            .Select(node => node.Id)
            .SingleOrDefaultAsync();
    }
}
