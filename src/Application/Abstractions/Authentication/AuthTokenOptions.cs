namespace Application.Abstractions.Authentication;

public class AuthTokenOptions
{
    public int AccessTokenTtlMinutes { get; set; } = 15;

    public int RefreshTokenTtlDays { get; set; } = 30;

    public string RefreshTokenPepper { get; set; } = string.Empty;

    public bool UseRefreshTokenCookie { get; set; } = true;

    public string RefreshCookieName { get; set; } = "rt";

    public string RefreshCookieSameSite { get; set; } = "Lax";

    public string RefreshCookiePath { get; set; } = "/auth/refresh";
}
