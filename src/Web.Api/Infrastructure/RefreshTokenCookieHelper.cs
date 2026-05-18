using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Infrastructure;

public static class RefreshTokenCookieHelper
{
    public static void AppendRefreshTokenCookie(
        HttpResponse response,
        AuthTokenOptions options,
        string refreshToken,
        DateTime expiresAtUtc)
    {
        CookieOptions cookieOptions = BuildCookieOptions(response, options, expiresAtUtc);
        response.Cookies.Append(options.RefreshCookieName, refreshToken, cookieOptions);
    }

    public static void ClearRefreshTokenCookie(HttpResponse response, AuthTokenOptions options)
    {
        CookieOptions cookieOptions = BuildCookieOptions(response, options, expiresAtUtc: null);
        response.Cookies.Delete(options.RefreshCookieName, cookieOptions);
    }

    private static CookieOptions BuildCookieOptions(
        HttpResponse response,
        AuthTokenOptions options,
        DateTime? expiresAtUtc)
    {
        bool isHttps = response.HttpContext?.Request?.IsHttps ?? false;

        SameSiteMode sameSite = ParseSameSite(options.RefreshCookieSameSite);
        bool secure = ResolveSecure(options.RefreshCookieSecurePolicy, isHttps);

        // ⚠️ 瀏覽器規則：SameSite=None 必須 Secure=true，不然會被拒收/拒送
        if (sameSite == SameSiteMode.None && !secure)
        {
            // dev/http 情況下如果你誤設 None，直接降級，避免 cookie 被瀏覽器擋掉
            sameSite = SameSiteMode.Lax;
        }

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Expires = expiresAtUtc,
            Path = string.IsNullOrWhiteSpace(options.RefreshCookiePath) ? "/" : options.RefreshCookiePath,
            Domain = string.IsNullOrWhiteSpace(options.RefreshCookieDomain) ? null : options.RefreshCookieDomain
        };
    }

    private static bool ResolveSecure(string policy, bool isHttps)
    {
        // 支援 SameAsRequest / Always / Never
        if (string.Equals(policy, "Always", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(policy, "Never", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return isHttps; // SameAsRequest（預設）
    }

    private static SameSiteMode ParseSameSite(string value)
    {
        return Enum.TryParse(value, true, out SameSiteMode mode)
            ? mode
            : SameSiteMode.Lax;
    }
}
