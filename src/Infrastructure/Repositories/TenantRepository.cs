using Domain.Security;
using Domain.Tenants;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TenantRepository(ApplicationDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        ResourceNode? node = await context.ResourceNodes
            .AsNoTracking()
            .FirstOrDefaultAsync(resourceNode => resourceNode.Id == tenantId, cancellationToken);

        return node is null ? null : Tenant.Create(node.Id, node.ExternalKey, node.Name);
    }

    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        ResourceNode? node = await context.ResourceNodes
            .AsNoTracking()
            .FirstOrDefaultAsync(resourceNode => resourceNode.ExternalKey == code, cancellationToken);

        return node is null ? null : Tenant.Create(node.Id, node.ExternalKey, node.Name);
    }
}
