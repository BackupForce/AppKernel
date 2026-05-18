namespace Domain.Security;

public sealed partial class Permission
{
    /// <summary>
    /// 針對使用者列表UserList的權限定義
    /// </summary>
    public static class Users
    {
        public static readonly Permission All = new(0, "USERS:*", "使用者模組所有權限", PermissionScope.Tenant);
        public static readonly Permission View = new(1, "USERS:READ", "檢視使用者資料", PermissionScope.Tenant);
        public static readonly Permission Create = new(2, "USERS:CREATE", "建立使用者", PermissionScope.Tenant);
        public static readonly Permission Update = new(3, "USERS:UPDATE", "修改使用者", PermissionScope.Tenant);
        public static readonly Permission Delete = new(4, "USERS:DELETE", "刪除使用者", PermissionScope.Tenant);
        public static readonly Permission ResetPassword = new(5, "USERS:RESET_PASSWORD", "重設密碼", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Delete, ResetPassword
        };

        public static IEnumerable<Permission> ChildPermissions => AllPermissions.Where(p => p != All);
    }
}
