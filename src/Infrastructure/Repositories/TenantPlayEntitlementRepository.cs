using Domain.Gaming.Catalog;
using Domain.Gaming.Entitlements;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TenantPlayEntitlementRepository(ApplicationDbContext context) : ITenantPlayEntitlementRepository
{
    public Task<TenantPlayEntitlement?> GetAsync(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        CancellationToken cancellationToken = default)
    {
        return context.TenantPlayEntitlements
            .AsTracking()
            .FirstOrDefaultAsync(
                entitlement => entitlement.TenantId == tenantId
                               && entitlement.GameCode == gameCode
                               && entitlement.PlayTypeCode == playTypeCode,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantPlayEntitlement>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await context.TenantPlayEntitlements
            .AsNoTracking()
            .Where(entitlement => entitlement.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public void Insert(TenantPlayEntitlement entitlement)
    {
        context.TenantPlayEntitlements.Add(entitlement);
    }

    public void Update(TenantPlayEntitlement entitlement)
    {
        context.TenantPlayEntitlements.Update(entitlement);
    }
}
