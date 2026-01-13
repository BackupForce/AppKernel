using System.Data;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Tenants;
using Dapper;

namespace Infrastructure.Tenants;

internal sealed class TenantTimeZoneProvider(
    ICacheService cacheService,
    IDbConnectionFactory dbConnectionFactory) : ITenantTimeZoneProvider
{
    private const string CacheKeyPrefix = "tenant:";
    private const string DefaultTimeZoneId = "UTC";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(20);

    public async Task<string> GetTimeZoneIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        string cacheKey = $"{CacheKeyPrefix}{tenantId}:timezone";
        string? cachedTimeZoneId = await cacheService.GetAsync<string>(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedTimeZoneId))
        {
            return cachedTimeZoneId;
        }

        const string sql =
            """
            SELECT time_zone_id
            FROM tenants
            WHERE id = @TenantId
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        string? timeZoneId = await connection.QueryFirstOrDefaultAsync<string>(
            sql,
            new { TenantId = tenantId });

        string resolvedTimeZoneId = string.IsNullOrWhiteSpace(timeZoneId)
            ? DefaultTimeZoneId
            : timeZoneId.Trim();

        await cacheService.SetAsync(cacheKey, resolvedTimeZoneId, CacheTtl, cancellationToken);

        return resolvedTimeZoneId;
    }
}
