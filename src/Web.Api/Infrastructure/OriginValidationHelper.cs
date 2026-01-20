using Microsoft.AspNetCore.Http;

namespace Web.Api.Infrastructure;

public static class OriginValidationHelper
{
    public static bool IsSameOrigin(HttpRequest request)
    {
        string? origin = request.Headers.Origin;
        string? referer = request.Headers.Referer;
        string host = request.Host.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(origin) && string.IsNullOrWhiteSpace(referer))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(origin))
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? originUri))
            {
                return false;
            }

            return IsSameHost(request, originUri, host);
        }

        if (!string.IsNullOrWhiteSpace(referer))
        {
            if (!Uri.TryCreate(referer, UriKind.Absolute, out Uri? refererUri))
            {
                return false;
            }

            return IsSameHost(request, refererUri, host);
        }

        return false;
    }

    private static bool IsSameHost(HttpRequest request, Uri uri, string host)
    {
        string requestScheme = request.Scheme;
        return string.Equals(uri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase)
            && string.Equals(uri.Scheme, requestScheme, StringComparison.OrdinalIgnoreCase)
            && (request.Host.Port == uri.Port || request.Host.Port is null && uri.IsDefaultPort)
            && string.Equals(uri.Authority, host, StringComparison.OrdinalIgnoreCase);
    }
}
