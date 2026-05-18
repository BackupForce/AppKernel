namespace Application.Abstractions.Gaming;

public interface IEntitlementCacheInvalidator
{
    Task InvalidateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
