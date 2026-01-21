using SharedKernel;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    public static readonly Error InvalidCredentials = Error.NotFound(
    "Users.InvalidCredentials",
    "Invalid email or password.");

    public static Error RoleAlreadyAssigned(int roleId) => Error.Conflict(
        "Users.RoleAlreadyAssigned",
        $"使用者已擁有角色 (Id = {roleId})。");

    public static readonly Error RoleAssignmentNotAllowed = Error.Forbidden(
        "Users.RoleAssignmentNotAllowed",
        "依照使用者類型與租戶規則，禁止指派該角色。");

    public static Error GroupAlreadyAssigned(Guid groupId) => Error.Conflict(
        "Users.GroupAlreadyAssigned",
        $"使用者已擁有群組 (Id = {groupId})。");

    public static Error GroupNotAssigned(Guid groupId) => Error.NotFound(
        "Users.GroupNotAssigned",
        $"使用者未指派群組 (Id = {groupId})。");

    public static readonly Error UserTypeInvalid = Error.Validation(
        "Users.UserTypeInvalid",
        "使用者類型無效，請確認輸入值。");

    public static readonly Error TenantIdRequired = Error.Validation(
        "Users.TenantIdRequired",
        "租戶使用者必須指定 TenantId。");

    public static readonly Error LoginProviderKeyRequired = Error.Validation(
        "Users.LoginProviderKeyRequired",
        "登入識別值不可為空。");

    public static Error LoginProviderAlreadyBound(LoginProvider provider) => Error.Conflict(
        "Users.LoginProviderAlreadyBound",
        $"登入方式 {provider} 已綁定其他識別值。");

    public static Error LoginProviderNotBound(LoginProvider provider) => Error.NotFound(
        "Users.LoginProviderNotBound",
        $"登入方式 {provider} 尚未綁定。");

    public static readonly Error LoginBindingRequired = Error.Validation(
        "Users.LoginBindingRequired",
        "使用者至少需要一種登入方式。");
}
