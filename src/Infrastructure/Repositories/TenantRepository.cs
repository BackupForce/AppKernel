using Domain.Tenants;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TenantRepository(ApplicationDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);

        return tenant is null ? null : Tenant.Create(tenant.Id, tenant.Code, tenant.Name);
    }

    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(tenant => tenant.Code == code, cancellationToken);

        return tenant is null ? null : Tenant.Create(tenant.Id, tenant.Code, tenant.Name);
    }
}
