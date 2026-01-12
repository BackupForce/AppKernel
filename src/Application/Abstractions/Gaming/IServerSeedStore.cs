namespace Application.Abstractions.Gaming;

/// <summary>
/// ServerSeed 暫存介面，避免在 Domain 層暴露基礎設施。
/// </summary>
public interface IServerSeedStore
{
    /// <summary>
    /// 保存 ServerSeed，配合 TTL 控制揭露時間與資料清理。
    /// </summary>
    Task StoreAsync(Guid drawId, string serverSeed, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得 ServerSeed 以進行開獎揭露與驗證。
    /// </summary>
    Task<string?> GetAsync(Guid drawId, CancellationToken cancellationToken = default);
}
