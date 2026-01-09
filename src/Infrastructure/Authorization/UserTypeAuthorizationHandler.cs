using Application.Abstractions.Authentication;
using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authorization;

internal sealed class UserTypeAuthorizationHandler : AuthorizationHandler<UserTypeRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTypeAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserTypeRequirement requirement)
    {
        if (!JwtUserContext.TryFromClaims(context.User, out JwtUserContext? jwtContext) || jwtContext is null)
        {
            return Task.CompletedTask;
        }

        if (!requirement.AllowedTypes.Contains(jwtContext.UserType))
        {
            return Task.CompletedTask;
        }

        if (requirement.EnforceTenantMatch && jwtContext.UserType != UserType.Platform)
        {
            Guid? routeTenantId = ResolveTenantId(context);
            if (routeTenantId.HasValue && jwtContext.TenantId.HasValue
                && routeTenantId.Value != jwtContext.TenantId.Value)
            {
                return Task.CompletedTask;
            }
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }

    private Guid? ResolveTenantId(AuthorizationHandlerContext context)
    {
        HttpContext? httpContext = context.Resource as HttpContext ?? _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.RouteValues.TryGetValue("tenantId", out object? tenantValue)
            && TryGetGuid(tenantValue, out Guid tenantId))
        {
            return tenantId;
        }

        if (httpContext.Items.TryGetValue("TenantId", out object? tenantItem)
            && TryGetGuid(tenantItem, out Guid resolvedTenantId))
        {
            return resolvedTenantId;
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
