namespace Domain.Security;

public sealed partial class Permission
{
    /// <summary>
    /// 租戶管理權限定義
    /// </summary>
    public static class Tenants
    {
        public static readonly Permission All = new(60, "TENANTS:*", "租戶模組所有權限", PermissionScope.Platform);
        public static readonly Permission Create = new(61, "TENANTS:CREATE", "建立租戶", PermissionScope.Platform);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Create
        };
    }
}
