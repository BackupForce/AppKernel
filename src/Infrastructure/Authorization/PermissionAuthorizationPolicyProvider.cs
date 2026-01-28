using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    private static readonly ConcurrentDictionary<string, AuthorizationPolicy> PolicyCache = new();

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // 先交給內建 provider（只讀，thread-safe）
        AuthorizationPolicy? basePolicy = await base.GetPolicyAsync(policyName);
        if (basePolicy is not null)
        {
            return basePolicy;
        }

        // 動態 permission policy（不寫入 AuthorizationOptions）
        AuthorizationPolicy policy = PolicyCache.GetOrAdd(
            policyName,
            static name =>
                new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(name))
                    .Build());

        return policy;
    }
}
