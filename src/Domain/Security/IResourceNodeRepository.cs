namespace Domain.Security;

public interface IResourceNodeRepository
{
    Task<ResourceNode?> GetByExternalKeyAsync(Guid tenantId, string externalKey, CancellationToken cancellationToken);
}
