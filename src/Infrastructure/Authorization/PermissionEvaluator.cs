using Application.Abstractions.Authorization;
using AppPermissionCheckContext = Application.Abstractions.Authorization.PermissionCheckContext;
using Domain.Security;

namespace Infrastructure.Authorization;

public sealed class PermissionEvaluator : IPermissionEvaluator
{
    private readonly IGrantedPermissionProvider _grantedPermissionProvider;
    private static readonly IReadOnlyDictionary<string, string[]> PermissionAliases = new Dictionary<string, string[]>
    {
        {
            NormalizePermissionCode("gaming.drawgroup.read"),
            new[] { NormalizePermissionCode("gaming.campaign.read") }
        },
        {
            NormalizePermissionCode("gaming.drawgroup.create"),
            new[] { NormalizePermissionCode("gaming.campaign.create") }
        },
        {
            NormalizePermissionCode("gaming.drawgroup.update"),
            new[] { NormalizePermissionCode("gaming.campaign.update") }
        },
        {
            NormalizePermissionCode("gaming.drawgroup.activate"),
            new[] { NormalizePermissionCode("gaming.campaign.activate") }
        },
        {
            NormalizePermissionCode("gaming.drawgroup.end"),
            new[] { NormalizePermissionCode("gaming.campaign.end") }
        },
        {
            NormalizePermissionCode("gaming.drawgroup.delete"),
            new[] { NormalizePermissionCode("gaming.campaign.delete") }
        },
        {
            NormalizePermissionCode("gaming.drawgroup.draw.manage"),
            new[] { NormalizePermissionCode("gaming.campaign.draw.manage") }
        }
    };

    public PermissionEvaluator(IGrantedPermissionProvider grantedPermissionProvider)
    {
        _grantedPermissionProvider = grantedPermissionProvider;
    }

    public async Task<bool> AuthorizeAsync(
        AppPermissionCheckContext context,
        CallerContext callerContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.PermissionCode))
        {
            return false;
        }

        string requiredPermission = NormalizePermissionCode(context.PermissionCode);
        IReadOnlySet<string> grantedPermissions;

        switch (context.Scope)
        {
            case PermissionScope.Platform:
                grantedPermissions = await _grantedPermissionProvider.GetPlatformPermissionsAsync(
                    callerContext.CallerUserId,
                    cancellationToken);
                break;
            case PermissionScope.Tenant:
                if (!context.TenantId.HasValue)
                {
                    return false;
                }

                grantedPermissions = await _grantedPermissionProvider.GetTenantPermissionsAsync(
                    callerContext.CallerUserId,
                    context.TenantId.Value,
                    cancellationToken);
                break;
            default:
                return false;
        }

        return HasPermission(grantedPermissions, requiredPermission);
    }

    private static bool HasPermission(IReadOnlySet<string> grantedPermissions, string requiredPermission)
    {
        if (grantedPermissions.Count == 0)
        {
            return false;
        }

        IReadOnlyCollection<string> requiredCodes = GetRequiredCodes(requiredPermission);

        foreach (string grantedPermission in grantedPermissions)
        {
            if (string.IsNullOrWhiteSpace(grantedPermission))
            {
                continue;
            }

            string normalizedGranted = NormalizePermissionCode(grantedPermission);
            if (requiredCodes.Contains(normalizedGranted))
            {
                return true;
            }

            if (normalizedGranted.EndsWith(":*", StringComparison.Ordinal))
            {
                string prefix = normalizedGranted[..^1];
                if (requiredCodes.Any(code => code.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IReadOnlyCollection<string> GetRequiredCodes(string requiredPermission)
    {
        if (PermissionAliases.TryGetValue(requiredPermission, out string[]? aliases))
        {
            string[] combined = new string[aliases.Length + 1];
            combined[0] = requiredPermission;
            Array.Copy(aliases, 0, combined, 1, aliases.Length);
            return combined;
        }

        return new[] { requiredPermission };
    }

    private static string NormalizePermissionCode(string permissionCode)
    {
        return permissionCode.Trim().ToUpperInvariant();
    }
}
