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
}
