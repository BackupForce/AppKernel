namespace Domain.Security;

public sealed partial class Permission
{
    /// <summary>
    /// 會員模組權限定義
    /// </summary>
    public static class Members
    {
        public static readonly Permission All = new(10, "MEMBERS:*", "會員模組所有權限", PermissionScope.Tenant);
        public static readonly Permission View = new(11, "MEMBERS:VIEW", "檢視會員資料", PermissionScope.Tenant);
        public static readonly Permission Create = new(12, "MEMBERS:CREATE", "建立會員", PermissionScope.Tenant);
        public static readonly Permission Update = new(13, "MEMBERS:UPDATE", "修改會員資料", PermissionScope.Tenant);
        public static readonly Permission Suspend = new(14, "MEMBERS:SUSPEND", "停權會員", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Suspend
        };
    }
}
