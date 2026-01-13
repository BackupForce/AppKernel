using Domain.Tenants;
using Microsoft.Extensions.Options;
using Web.Api.Common;
using Web.Api.Settings;

namespace Web.Api.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    private const string TenantIdHeaderName = "X-Tenant-Id";
    private const string TenantCodeHeaderName = "X-Tenant-Code";

    public async Task Invoke(
        HttpContext context,
        ITenantRepository tenantRepository,
        IOptions<TenantResolutionOptions> tenantResolutionOptions)
    {
        context.Items.Remove("TenantId");

        Guid? tenantIdFromRoute = TryResolveTenantIdFromRoute(context);
        Guid? tenantIdFromHeader = TryResolveTenantIdFromHeader(context);
        string? tenantCodeFromHeader = TryResolveTenantCode(context);

        if (tenantIdFromRoute.HasValue)
        {
            if (tenantIdFromHeader.HasValue && tenantIdFromHeader.Value != tenantIdFromRoute.Value)
            {
                await WriteProblemDetailsAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    "TenantId 不一致",
                    "路由 tenantId 與 X-Tenant-Id 不一致。",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1");
                return;
            }

            if (!string.IsNullOrWhiteSpace(tenantCodeFromHeader))
            {
                string normalizedCode = TenantCodeHelper.Normalize(tenantCodeFromHeader);
                if (!TenantCodeHelper.IsValid(normalizedCode))
                {
                    await WriteProblemDetailsAsync(
                        context,
                        StatusCodes.Status400BadRequest,
                        "TenantCode 格式錯誤",
                        "X-Tenant-Code 格式錯誤。",
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1");
                    return;
                }

                Tenant? tenantByCode = await tenantRepository.GetByCodeAsync(normalizedCode, context.RequestAborted);
                if (tenantByCode is null || tenantByCode.Id != tenantIdFromRoute.Value)
                {
                    await WriteProblemDetailsAsync(
                        context,
                        StatusCodes.Status400BadRequest,
                        "Tenant 資訊不一致",
                        "路由 tenantId 與 X-Tenant-Code 不一致。",
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1");
                    return;
                }
            }

            Tenant? routeTenant = await tenantRepository.GetByIdAsync(tenantIdFromRoute.Value, context.RequestAborted);
            if (routeTenant is null)
            {
                await WriteProblemDetailsAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    "Tenant 找不到",
                    "指定的 tenantId 不存在。",
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4");
                return;
            }

            context.Items["TenantId"] = routeTenant.Id;
            await next(context);
            return;
        }

        TenantResolutionOptions options = tenantResolutionOptions.Value;
        Guid? resolvedTenantId = null;

        if (options.AllowTenantIdHeader && tenantIdFromHeader.HasValue)
        {
            resolvedTenantId = tenantIdFromHeader.Value;
        }

        if (!resolvedTenantId.HasValue && !string.IsNullOrWhiteSpace(tenantCodeFromHeader))
        {
            string normalizedCode = TenantCodeHelper.Normalize(tenantCodeFromHeader);
            if (TenantCodeHelper.IsValid(normalizedCode))
            {
                Tenant? tenantByCode = await tenantRepository.GetByCodeAsync(normalizedCode, context.RequestAborted);
                if (tenantByCode is not null)
                {
                    resolvedTenantId = tenantByCode.Id;
                }
            }
        }

        if (resolvedTenantId.HasValue)
        {
            context.Items["TenantId"] = resolvedTenantId.Value;
        }

        await next(context);
    }

    private static Guid? TryResolveTenantIdFromRoute(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("tenantId", out object? tenantValue) && TryGetGuid(tenantValue, out Guid tenantId))
        {
            return tenantId;
        }

        return null;
    }

    private static Guid? TryResolveTenantIdFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantIdHeaderName, out Microsoft.Extensions.Primitives.StringValues tenantIdValues) && Guid.TryParse(tenantIdValues.FirstOrDefault(), out Guid tenantId))
        {
            return tenantId;
        }

        return null;
    }

    private static string? TryResolveTenantCode(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantCodeHeaderName, out Microsoft.Extensions.Primitives.StringValues tenantCodeValues))
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

    private static Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        string type)
    {
        IResult result = Results.Problem(
            title: title,
            detail: detail,
            type: type,
            statusCode: statusCode);
        return result.ExecuteAsync(context);
    }
}
