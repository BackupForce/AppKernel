using Application.Abstractions.Caching;
using Application.Abstractions.Gaming;
using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Gaming;

internal sealed class EntitlementChecker(ApplicationDbContext dbContext, ICacheService cacheService) : IEntitlementChecker
{
    private const string EntitlementCachePrefix = "tenant:";
    private const string EntitlementCacheSuffix = ":entitlements";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public async Task<Result> EnsureGameEnabledAsync(Guid tenantId, GameCode gameCode, CancellationToken cancellationToken = default)
    {
        TenantEntitlementCache cache = await GetCacheAsync(tenantId, cancellationToken);

        if (cache.EnabledGameCodes.Contains(gameCode.Value))
        {
            return Result.Success();
        }

        return Result.Failure(GamingErrors.GameNotEntitled);
    }

    public async Task<Result> EnsurePlayEnabledAsync(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        CancellationToken cancellationToken = default)
    {
        TenantEntitlementCache cache = await GetCacheAsync(tenantId, cancellationToken);

        if (!cache.EnabledGameCodes.Contains(gameCode.Value))
        {
            return Result.Failure(GamingErrors.GameNotEntitled);
        }

        if (!cache.EnabledPlayTypesByGame.TryGetValue(gameCode.Value, out HashSet<string>? playTypes)
            || !playTypes.Contains(playTypeCode.Value))
        {
            return Result.Failure(GamingErrors.PlayNotEntitled);
        }

        return Result.Success();
    }

    public async Task<TenantEntitlementsDto> GetTenantEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        TenantEntitlementCache cache = await GetCacheAsync(tenantId, cancellationToken);
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> playMap = cache.EnabledPlayTypesByGame
            .ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyCollection<string>)pair.Value.OrderBy(value => value).ToList());

        return new TenantEntitlementsDto(
            cache.EnabledGameCodes.OrderBy(value => value).ToList(),
            playMap);
    }

    private async Task<TenantEntitlementCache> GetCacheAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        string cacheKey = $"{EntitlementCachePrefix}{tenantId:D}{EntitlementCacheSuffix}";
        TenantEntitlementCache? cached = await cacheService.GetAsync<TenantEntitlementCache>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        List<TenantGameEntitlement> gameEntitlements = await dbContext.TenantGameEntitlements
            .AsNoTracking()
            .Where(entitlement => entitlement.TenantId == tenantId && entitlement.IsEnabled)
            .ToListAsync(cancellationToken);

        List<TenantPlayEntitlement> playEntitlements = await dbContext.TenantPlayEntitlements
            .AsNoTracking()
            .Where(entitlement => entitlement.TenantId == tenantId && entitlement.IsEnabled)
            .ToListAsync(cancellationToken);

        TenantEntitlementCache cache = new TenantEntitlementCache();
        foreach (TenantGameEntitlement game in gameEntitlements)
        {
            cache.EnabledGameCodes.Add(game.GameCode.Value);
        }

        foreach (TenantPlayEntitlement play in playEntitlements)
        {
            if (!cache.EnabledPlayTypesByGame.TryGetValue(play.GameCode.Value, out HashSet<string>? plays))
            {
                plays = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                cache.EnabledPlayTypesByGame[play.GameCode.Value] = plays;
            }

            plays.Add(play.PlayTypeCode.Value);
        }

        await cacheService.SetAsync(cacheKey, cache, CacheTtl, cancellationToken);

        return cache;
    }

    private sealed class TenantEntitlementCache
    {
        public HashSet<string> EnabledGameCodes { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, HashSet<string>> EnabledPlayTypesByGame { get; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
