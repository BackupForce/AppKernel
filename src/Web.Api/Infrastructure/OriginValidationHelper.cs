using Microsoft.AspNetCore.Http;

namespace Web.Api.Infrastructure;

public static class OriginValidationHelper
{
    public static bool IsAllowedOrigin(HttpRequest request, IReadOnlyCollection<string> allowedOrigins)
    {
        string? origin = request.Headers.Origin;
        string? referer = request.Headers.Referer;

        // 非瀏覽器情境（例如 server-to-server / curl 沒帶 Origin/Referer）
        // 你要嚴格一點也可以改成 false
        if (string.IsNullOrWhiteSpace(origin) && string.IsNullOrWhiteSpace(referer))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(origin))
        {
            return IsInAllowedList(origin, allowedOrigins);
        }

        if (!string.IsNullOrWhiteSpace(referer))
        {
            if (!Uri.TryCreate(referer, UriKind.Absolute, out Uri? refererUri))
            {
                return false;
            }

            // referer 只取 scheme://host[:port]
            string refererOrigin = $"{refererUri.Scheme}://{refererUri.Authority}";
            return IsInAllowedList(refererOrigin, allowedOrigins);
        }

        return false;
    }

    private static bool IsInAllowedList(string origin, IReadOnlyCollection<string> allowedOrigins)
    {
        // 允許清單建議用「精準 origin」，不要用 wildcard
        foreach (string allowed in allowedOrigins)
        {
            if (string.Equals(origin.TrimEnd('/'), allowed.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
