namespace Domain.Security;

public interface IResourceNodeRepository
{
    Task<ResourceNode?> GetByExternalKeyAsync(string externalKey, CancellationToken cancellationToken);
}
