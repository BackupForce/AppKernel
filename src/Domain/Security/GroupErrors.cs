using SharedKernel;

namespace Domain.Security;

public static class GroupErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Group.NameRequired",
        "群組名稱不可為空白。");

    public static readonly Error NameConflict = Error.Conflict(
        "Group.NameConflict",
        "群組名稱已存在。");

    public static readonly Error NotFound = Error.NotFound(
        "Group.NotFound",
        "找不到群組。");
}
