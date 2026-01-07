using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ResourceNodeRepository(ApplicationDbContext context) : IResourceNodeRepository
{
    public Task<ResourceNode?> GetByExternalKeyAsync(Guid tenantId, string externalKey, CancellationToken cancellationToken)
    {
        return context.ResourceNodes
            .AsNoTracking()
            .FirstOrDefaultAsync(node => node.TenantId == tenantId && node.ExternalKey == externalKey, cancellationToken);
    }

    public Task<Guid?> GetRootNodeIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return context.ResourceNodes
            .AsNoTracking()
            .Where(node => node.TenantId == tenantId && node.ParentId == null)
            .Select(node => (Guid?)node.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public void Insert(ResourceNode node)
    {
        context.ResourceNodes.Add(node);
    }
}
