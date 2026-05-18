namespace Application.Abstractions.Authentication;

public class AuthTokenOptions
{
    public int AccessTokenTtlMinutes { get; set; } = 15;
    public int RefreshTokenTtlDays { get; set; } = 30;
    public string RefreshTokenPepper { get; set; } = string.Empty;

    public bool UseRefreshTokenCookie { get; set; } = true;

    public string RefreshCookieName { get; set; } = "rt";
    public string RefreshCookieSameSite { get; set; } = "Lax";
    public string RefreshCookiePath { get; set; } = "/";

    // ✅ 新增：prod 可設 ".backupforcetw.com"，dev 留空或 null
    public string? RefreshCookieDomain { get; set; }

    // ✅ 新增：SameAsRequest / Always / Never
    // - Development 建議 SameAsRequest（http -> false, https -> true）
    // - Production 建議 Always
    public string RefreshCookieSecurePolicy { get; set; } = "SameAsRequest";
}
