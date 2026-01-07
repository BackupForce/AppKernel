using Domain.Tenants;

namespace Web.Api.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    private const string TenantIdHeaderName = "X-Tenant-Id";
    private const string TenantCodeHeaderName = "X-Tenant-Code";

    public async Task Invoke(HttpContext context, ITenantRepository tenantRepository)
    {
        Guid? tenantId = TryResolveTenantIdFromRoute(context) ?? TryResolveTenantIdFromHeader(context);

        if (!tenantId.HasValue)
        {
            string? tenantCode = TryResolveTenantCode(context);
            if (!string.IsNullOrWhiteSpace(tenantCode) && IsValidTenantCode(tenantCode))
            {
                Tenant? tenant = await tenantRepository.GetByCodeAsync(tenantCode, context.RequestAborted);
                if (tenant is not null)
                {
                    tenantId = tenant.Id;
                }
            }
        }
        else
        {
            Tenant? tenant = await tenantRepository.GetByIdAsync(tenantId.Value, context.RequestAborted);
            if (tenant is null)
            {
                tenantId = null;
            }
        }

        if (tenantId.HasValue)
        {
            context.Items["TenantId"] = tenantId.Value;
        }

        await next(context);
    }

    private static Guid? TryResolveTenantIdFromRoute(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("tenantId", out object? tenantValue))
        {
            if (TryGetGuid(tenantValue, out Guid tenantId))
            {
                return tenantId;
            }
        }

        return null;
    }

    private static Guid? TryResolveTenantIdFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantIdHeaderName, out var tenantIdValues))
        {
            if (Guid.TryParse(tenantIdValues.FirstOrDefault(), out Guid tenantId))
            {
                return tenantId;
            }
        }

        return null;
    }

    private static string? TryResolveTenantCode(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantCodeHeaderName, out var tenantCodeValues))
        {
            return tenantCodeValues.FirstOrDefault();
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

    private static bool IsValidTenantCode(string tenantCode)
    {
        if (tenantCode.Length != 3)
        {
            return false;
        }

        for (int index = 0; index < tenantCode.Length; index++)
        {
            if (!char.IsLetterOrDigit(tenantCode[index]))
            {
                return false;
            }
        }

        return true;
    }
}
