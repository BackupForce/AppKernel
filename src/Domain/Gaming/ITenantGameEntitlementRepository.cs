namespace Domain.Gaming;

public interface ITenantGameEntitlementRepository
{
    Task<TenantGameEntitlement?> GetAsync(Guid tenantId, GameCode gameCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantGameEntitlement>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    void Insert(TenantGameEntitlement entitlement);

    void Update(TenantGameEntitlement entitlement);
}
