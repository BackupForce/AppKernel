using Application.Abstractions.Caching;
using Application.Abstractions.Gaming;

namespace Infrastructure.Gaming;

internal sealed class ServerSeedStore(ICacheService cacheService) : IServerSeedStore
{
    private const string CacheKeyPrefix = "gaming:lottery539:server-seed:";

    public async Task StoreAsync(Guid drawId, string serverSeed, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        string key = GetKey(drawId);
        await cacheService.SetAsync(key, serverSeed, ttl, cancellationToken);
    }

    public async Task<string?> GetAsync(Guid drawId, CancellationToken cancellationToken = default)
    {
        string key = GetKey(drawId);
        return await cacheService.GetAsync<string>(key, cancellationToken);
    }

    private static string GetKey(Guid drawId)
    {
        return $"{CacheKeyPrefix}{drawId}";
    }
}
