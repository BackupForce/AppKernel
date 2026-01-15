using Application.Abstractions.Caching;
using Domain.Gaming;
using FluentAssertions;
using Infrastructure.Database;
using Infrastructure.Gaming;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.UnitTests.Gaming;

public class EntitlementCheckerTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task EnsureGameEnabledAsync_Should_ReturnFailure_WhenGameNotEnabled()
    {
        await using ApplicationDbContext dbContext = CreateDbContext();
        ICacheService cacheService = new InMemoryCacheService();
        EntitlementChecker checker = new EntitlementChecker(dbContext, cacheService);

        Result result = await checker.EnsureGameEnabledAsync(Guid.NewGuid(), GameCodes.Lottery539);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.GameNotEntitled);
    }

    [Fact]
    public async Task EnsurePlayEnabledAsync_Should_ReturnSuccess_WhenGameAndPlayEnabled()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        await using ApplicationDbContext dbContext = CreateDbContext();
        dbContext.TenantGameEntitlements.Add(TenantGameEntitlement.Create(tenantId, GameCodes.Lottery539, now));
        dbContext.TenantPlayEntitlements.Add(TenantPlayEntitlement.Create(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic, now));
        await dbContext.SaveChangesAsync();

        ICacheService cacheService = new InMemoryCacheService();
        EntitlementChecker checker = new EntitlementChecker(dbContext, cacheService);

        Result result = await checker.EnsurePlayEnabledAsync(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnsurePlayEnabledAsync_Should_ReturnGameNotEntitled_WhenGameDisabled()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        await using ApplicationDbContext dbContext = CreateDbContext();
        TenantGameEntitlement gameEntitlement = TenantGameEntitlement.Create(tenantId, GameCodes.Lottery539, now);
        gameEntitlement.Disable(now);
        dbContext.TenantGameEntitlements.Add(gameEntitlement);
        dbContext.TenantPlayEntitlements.Add(TenantPlayEntitlement.Create(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic, now));
        await dbContext.SaveChangesAsync();

        ICacheService cacheService = new InMemoryCacheService();
        EntitlementChecker checker = new EntitlementChecker(dbContext, cacheService);

        Result result = await checker.EnsurePlayEnabledAsync(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.GameNotEntitled);
    }

    private sealed class InMemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _cache = new();

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out object? value) && value is T typed)
            {
                return Task.FromResult<T?>(typed);
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            _cache[key] = value!;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
