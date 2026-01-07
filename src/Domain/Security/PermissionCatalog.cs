using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Security;

public static class PermissionCatalog
{
    public static IReadOnlyCollection<string> AllPermissionCodes { get; } = BuildAllPermissionCodes();

    private static string[] BuildAllPermissionCodes()
    {
        // 中文註解：彙整所有模組的權限代碼並去重。
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Permission> permissions =
            Permission.Users.AllPermissions
                .Concat(Permission.Members.AllPermissions)
                .Concat(Permission.MemberPoints.AllPermissions)
                .Concat(Permission.MemberAssets.AllPermissions)
                .Concat(Permission.MemberAudit.AllPermissions)
                .Concat(Permission.Roles.AllPermissions);

        foreach (Permission permission in permissions)
        {
            if (string.IsNullOrWhiteSpace(permission.Name))
            {
                continue;
            }

            codes.Add(permission.Name);
        }

        return codes.ToArray();
    }

}
