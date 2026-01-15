using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TenantGameEntitlementRepository(ApplicationDbContext context) : ITenantGameEntitlementRepository
{
    public Task<TenantGameEntitlement?> GetAsync(Guid tenantId, GameCode gameCode, CancellationToken cancellationToken = default)
    {
        return context.TenantGameEntitlements
            .AsTracking()
            .FirstOrDefaultAsync(
                entitlement => entitlement.TenantId == tenantId && entitlement.GameCode == gameCode,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<TenantGameEntitlement>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await context.TenantGameEntitlements
            .AsNoTracking()
            .Where(entitlement => entitlement.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public void Insert(TenantGameEntitlement entitlement)
    {
        context.TenantGameEntitlements.Add(entitlement);
    }

    public void Update(TenantGameEntitlement entitlement)
    {
        context.TenantGameEntitlements.Update(entitlement);
    }
}
