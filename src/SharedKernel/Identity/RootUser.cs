namespace SharedKernel.Identity;

public static class RootUser
{
    public const string DefaultEmail = "root@system.local";

    public static bool Is(string email, string? configuredEmail = null)
    {
        return email.Equals(configuredEmail ?? DefaultEmail, StringComparison.OrdinalIgnoreCase);
    }
}
