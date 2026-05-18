using Application.Abstractions.Caching;
using Application.Abstractions.Gaming;

namespace Infrastructure.Gaming;

/// <summary>
/// ServerSeed 的快取實作，供 commit-reveal 流程使用。
/// </summary>
internal sealed class ServerSeedStore(ICacheService cacheService) : IServerSeedStore
{
    private const string CacheKeyPrefix = "gaming:lottery539:server-seed:";

    /// <summary>
    /// 以 TTL 儲存 seed，避免長期保存敏感資料。
    /// </summary>
    public async Task StoreAsync(Guid drawId, string serverSeed, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        string key = GetKey(drawId);
        await cacheService.SetAsync(key, serverSeed, ttl, cancellationToken);
    }

    /// <summary>
    /// 取得 seed 以供開獎揭露與驗證。
    /// </summary>
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
