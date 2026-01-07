using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ResourceNodeRepository(ApplicationDbContext context) : IResourceNodeRepository
{
    public Task<ResourceNode?> GetByExternalKeyAsync(string externalKey, CancellationToken cancellationToken)
    {
        return context.ResourceNodes
            .AsNoTracking()
            .FirstOrDefaultAsync(node => node.ExternalKey == externalKey, cancellationToken);
    }
}
