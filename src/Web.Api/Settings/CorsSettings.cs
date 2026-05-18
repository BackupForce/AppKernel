namespace Web.Api.Settings;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public IReadOnlyCollection<string> AllowedOrigins { get; init; } = Array.Empty<string>();

    public bool AllowCredentials { get; init; }
}
