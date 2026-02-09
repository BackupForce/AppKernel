namespace Domain.Security;

public static class RoleRestrictions
{
    public const string SuperAdmin = "SUPERADMIN";

    private static readonly HashSet<string> NonRemovableRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        SuperAdmin
    };

    public static bool IsRemovalRestricted(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        return NonRemovableRoles.Contains(roleName.Trim());
    }
}
