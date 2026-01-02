using SharedKernel;

namespace Application.Auth;

internal static class LineLoginErrors
{
    public static readonly Error LineUserIdRequired = Error.Validation(
        "Auth.LineUserIdRequired",
        "Line 使用者編號不可為空白。");

    public static readonly Error LineUserNameRequired = Error.Validation(
        "Auth.LineUserNameRequired",
        "Line 使用者名稱不可為空白。");

    public static readonly Error UserBindingMissing = Error.Problem(
        "Auth.LineUserBindingMissing",
        "Line 身分缺少對應的 User 紀錄。");
}
