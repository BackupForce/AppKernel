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
        response.Cookies.Append(
            options.RefreshCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false, //Todo:修改
                SameSite = ParseSameSite(options.RefreshCookieSameSite),
                Expires = expiresAtUtc,
                Path = options.RefreshCookiePath
            });
    }

    public static void ClearRefreshTokenCookie(HttpResponse response, AuthTokenOptions options)
    {
        response.Cookies.Delete(options.RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, //Todo:修改
            SameSite = ParseSameSite(options.RefreshCookieSameSite),
            Path = options.RefreshCookiePath
        });
    }

    private static SameSiteMode ParseSameSite(string value)
    {
        return Enum.TryParse(value, true, out SameSiteMode mode)
            ? mode
            : SameSiteMode.Lax;
    }
}
