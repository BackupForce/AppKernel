using SharedKernel;

namespace Application.Auth;

internal static class AuthErrors
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
}
