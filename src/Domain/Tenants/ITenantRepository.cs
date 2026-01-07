namespace Domain.Tenants;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
