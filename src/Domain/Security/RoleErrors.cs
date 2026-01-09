using SharedKernel;

namespace Domain.Security;

public static class RoleErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Role.NameRequired",
        "角色名稱不可為空白。");

    public static readonly Error NameConflict = Error.Conflict(
        "Role.NameConflict",
        "角色名稱已存在。");

    public static readonly Error NotFound = Error.NotFound(
        "Role.NotFound",
        "找不到角色。");

    public static readonly Error PermissionCodesRequired = Error.Validation(
        "Role.PermissionCodesRequired",
        "至少需要一個權限代碼。");

    public static readonly Error OperationNotAllowed = Error.Forbidden(
        "Role.OperationNotAllowed",
        "目前使用者無權執行角色操作。");

    public static readonly Error PermissionScopeMismatch = Error.Validation(
        "Role.PermissionScopeMismatch",
        "權限 Scope 與角色類型不一致。");
}
