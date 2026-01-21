using SharedKernel;

namespace Application.Auth;

public static class AuthErrors
{
    public static readonly Error TenantCodeRequired = Error.Validation(
        "Auth.TenantCodeRequired",
        "Tenant code is required.");

    public static readonly Error TenantCodeInvalidFormat = Error.Validation(
        "Auth.TenantCodeInvalidFormat",
        "Tenant code must be exactly 3 alphanumeric characters.");

    public static readonly Error TenantNotFound = Error.NotFound(
        "Auth.TenantNotFound",
        "Tenant not found.");

    public static readonly Error MemberLoginNotAllowed = Error.Validation(
        "Auth.MemberLoginNotAllowed",
        "會員帳號不得使用管理者登入流程。");

    public static readonly Error LineAccessTokenRequired = Error.Validation(
        "Auth.LineAccessTokenRequired",
        "LINE access token is required.");

    public static readonly Error LineVerifyFailed = Error.Validation(
        "Auth.LineVerifyFailed",
        "LINE 登入驗證失敗。");

    public static readonly Error LineUserIdMissing = Error.Validation(
        "Auth.LineUserIdMissing",
        "LINE 使用者識別碼缺失。");

    public static readonly Error LineLoginUserTypeInvalid = Error.Validation(
        "Auth.LineLoginUserTypeInvalid",
        "LINE 登入帳號類型不正確。");

    public static readonly Error TenantContextMissing = Error.Validation(
        "Auth.TenantContextMissing",
        "Tenant context is unavailable.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "invalid_refresh_token",
        "Refresh token is invalid.");

    public static readonly Error RefreshTokenExpired = Error.Unauthorized(
        "refresh_token_expired",
        "Refresh token is expired.");

    public static readonly Error RefreshTokenReused = Error.Unauthorized(
        "refresh_token_reused",
        "Refresh token reuse detected.");

    public static readonly Error SessionRevoked = Error.Unauthorized(
        "session_revoked",
        "Session is revoked.");

    public static Error SessionNotFound(Guid sessionId) => Error.NotFound(
        "Auth.SessionNotFound",
        $"Session with id '{sessionId}' was not found.");
}
