using System;
using System.Collections.Generic;
namespace Domain.Security;

public static class PermissionCatalog
{
    private static List<Permission> AllPermissions { get; } = BuildAllPermissions();
    public static IReadOnlyCollection<string> AllPermissionCodes { get; } = BuildAllPermissionCodes();
    public static IReadOnlyDictionary<string, PermissionScope> PermissionScopes { get; } = BuildPermissionScopes();


    private static string[] BuildAllPermissionCodes()
    {
        // 中文註解：彙整所有模組的權限代碼並去重。
        HashSet<string> codes = new HashSet<string>();

        foreach (Permission permission in AllPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            string normalizedCode = permission.Name.Trim().ToUpperInvariant();
            codes.Add(normalizedCode);
        }

        return codes.ToArray();
    }

    private static Dictionary<string, PermissionScope> BuildPermissionScopes()
    {
        Dictionary<string, PermissionScope> scopes = new Dictionary<string, PermissionScope>();

        foreach (Permission permission in AllPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            string normalizedCode = permission.Name.Trim().ToUpperInvariant();
            scopes[normalizedCode] = permission.Scope;
        }

        return scopes;
    }

    private static List<Permission> BuildAllPermissions()
    {
        List<Permission> permissions = new();

        void Add(string moduleName, IEnumerable<Permission>? source)
        {
            if (source is null)
            {
                throw new InvalidOperationException(
                    $"Permission module '{moduleName}' AllPermissions is null.");
            }

            if (source.Any(p => p is null))
            {
                throw new InvalidOperationException(
                    $"Permission module '{moduleName}' contains null permission definition.");
            }

            permissions.AddRange(source);
        }

        Add(nameof(Permission.Users), Permission.Users.AllPermissions);
        Add(nameof(Permission.Members), Permission.Members.AllPermissions);
        Add(nameof(Permission.MemberPoints), Permission.MemberPoints.AllPermissions);
        Add(nameof(Permission.MemberAssets), Permission.MemberAssets.AllPermissions);
        Add(nameof(Permission.MemberAudit), Permission.MemberAudit.AllPermissions);
        Add(nameof(Permission.Roles), Permission.Roles.AllPermissions);
        Add(nameof(Permission.Tenants), Permission.Tenants.AllPermissions);
        Add(nameof(Permission.Points), Permission.Points.AllPermissions);
        Add(nameof(Permission.Gaming), Permission.Gaming.AllPermissions);
        Add(nameof(Permission.Tickets), Permission.Tickets.AllPermissions);

        return permissions;
    }


    public static PermissionScope ResolveScope(string permissionCode)
    {
        if (TryGetScope(permissionCode, out PermissionScope scope))
        {
            return scope;
        }

        throw new InvalidOperationException($"未知的權限代碼，無法解析 Scope：{permissionCode}");
    }


    public static bool TryGetScope(string permissionCode, out PermissionScope scope)
    {
        scope = PermissionScope.Tenant;

        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        string normalizedCode = permissionCode.Trim().ToUpperInvariant();
        return PermissionScopes.TryGetValue(normalizedCode, out scope);
    }
}
