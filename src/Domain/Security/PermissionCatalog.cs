using System;
using System.Collections.Generic;
namespace Domain.Security;

public static class PermissionCatalog
{
    public static IReadOnlyCollection<string> AllPermissionCodes { get; } = BuildAllPermissionCodes();
    public static IReadOnlyDictionary<string, PermissionScope> PermissionScopes { get; } = BuildPermissionScopes();

    private static List<Permission> AllPermissions { get; } = BuildAllPermissions();

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
        List<Permission> permissions = new List<Permission>();

        permissions.AddRange(Permission.Users.AllPermissions);
        permissions.AddRange(Permission.Members.AllPermissions);
        permissions.AddRange(Permission.MemberPoints.AllPermissions);
        permissions.AddRange(Permission.MemberAssets.AllPermissions);
        permissions.AddRange(Permission.MemberAudit.AllPermissions);
        permissions.AddRange(Permission.Roles.AllPermissions);
        permissions.AddRange(Permission.Tenants.AllPermissions);
        permissions.AddRange(Permission.Points.AllPermissions);

        return permissions;
    }

    public static PermissionScope ResolveScope(string permissionCode)
    {
        if (TryGetScope(permissionCode, out PermissionScope scope))
        {
            return scope;
        }

        // 中文註解：找不到對應的權限定義時，預設視為租戶層級。
        return PermissionScope.Tenant;
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
