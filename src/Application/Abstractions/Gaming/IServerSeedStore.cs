namespace Application.Abstractions.Gaming;

public interface IServerSeedStore
{
    Task StoreAsync(Guid drawId, string serverSeed, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task<string?> GetAsync(Guid drawId, CancellationToken cancellationToken = default);
}
