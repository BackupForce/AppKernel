namespace Domain.Gaming;

public interface ITenantPlayEntitlementRepository
{
    Task<TenantPlayEntitlement?> GetAsync(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TenantPlayEntitlement>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    void Insert(TenantPlayEntitlement entitlement);

    void Update(TenantPlayEntitlement entitlement);
}
