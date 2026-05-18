namespace Web.Api.Settings;

public sealed class TenantResolutionOptions
{
    public const string SectionName = "TenantResolution";

    public bool AllowTenantIdHeader { get; init; }
}
