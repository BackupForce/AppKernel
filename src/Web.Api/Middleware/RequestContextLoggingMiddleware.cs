using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Web.Api.Middleware;

public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public Task Invoke(HttpContext context)
    {
        string userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";
        string traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        //可以新增參數方便用Seq Debug
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("UserId", userId))
        {
            return next.Invoke(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
