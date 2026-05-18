namespace Application.Abstractions.Authorization;

public static class AuthzCacheKeys
{
    public const string UserMatrixPrefix = "authz:matrix:";

    public static string ForUser(Guid userId) => $"{UserMatrixPrefix}{userId}";

    public static string ForUserTenant(Guid userId, Guid? tenantId)
    {
        string tenantPart = tenantId?.ToString("D") ?? "global";
        return $"{UserMatrixPrefix}{userId}:{tenantPart}";
    }
}
