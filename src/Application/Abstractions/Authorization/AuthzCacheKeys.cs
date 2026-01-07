namespace Application.Abstractions.Authorization;

public static class AuthzCacheKeys
{
    public const string UserMatrixPrefix = "authz:matrix:";

    public static string ForUser(Guid userId) => $"{UserMatrixPrefix}{userId}";
}
