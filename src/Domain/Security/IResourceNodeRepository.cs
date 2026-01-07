namespace Domain.Security;

public interface IResourceNodeRepository
{
    Task<ResourceNode?> GetByExternalKeyAsync(Guid tenantId, string externalKey, CancellationToken cancellationToken);

    Task<Guid?> GetRootNodeIdAsync(Guid tenantId, CancellationToken cancellationToken);

    void Insert(ResourceNode node);
}
