using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId =>
        TryResolveTenantId(_httpContextAccessor.HttpContext)
        ?? throw new ApplicationException("Tenant context is unavailable");

    public bool TryGetTenantId(out Guid tenantId)
    {
        Guid? resolved = TryResolveTenantId(_httpContextAccessor.HttpContext);
        if (resolved.HasValue)
        {
            tenantId = resolved.Value;
            return true;
        }

        tenantId = Guid.Empty;
        return false;
    }

    private static Guid? TryResolveTenantId(HttpContext? httpContext)
    {
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
