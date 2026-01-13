namespace Application.Abstractions.Tenants;

public interface ITenantTimeZoneProvider
{
    Task<string> GetTimeZoneIdAsync(Guid tenantId, CancellationToken cancellationToken);
}
