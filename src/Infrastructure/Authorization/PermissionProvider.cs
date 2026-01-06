using Application.Abstractions.Caching;

namespace Infrastructure.Authorization;

// 保留供未來改為快取取權限的實作使用，目前未在任何流程中被呼叫
internal sealed class PermissionProvider
{
    private readonly ICacheService _cacheService;

    public PermissionProvider(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        string cacheKey = $"auth:permissions-{userId}";
        HashSet<string>? cachedPermissions = await _cacheService.GetAsync<HashSet<string>>(cacheKey);

        if (cachedPermissions is not null)
        {
            return cachedPermissions;
        }

        // TODO: 未來改以快取為主來源
        var permissionsSet = new HashSet<string>();

        await _cacheService.SetAsync(cacheKey, permissionsSet);

        return permissionsSet;
    }
}
