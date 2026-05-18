using Application.Abstractions.Caching;
using Application.Abstractions.Gaming;

namespace Infrastructure.Gaming;

internal sealed class EntitlementCacheInvalidator(ICacheService cacheService) : IEntitlementCacheInvalidator
{
    private const string EntitlementCachePrefix = "tenant:";
    private const string EntitlementCacheSuffix = ":entitlements";

    public Task InvalidateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"{EntitlementCachePrefix}{tenantId:D}{EntitlementCacheSuffix}";
        return cacheService.RemoveAsync(cacheKey, cancellationToken);
    }
}
