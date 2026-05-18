namespace Domain.Security;

public sealed partial class Permission
{
    /// <summary>
    /// 角色管理權限定義
    /// </summary>
    public static class Roles
    {
        public static readonly Permission All = new(50, "ROLES:*", "角色模組所有權限", PermissionScope.Tenant);
        public static readonly Permission View = new(51, "ROLES:READ", "檢視角色", PermissionScope.Tenant);
        public static readonly Permission Create = new(52, "ROLES:CREATE", "建立角色", PermissionScope.Tenant);
        public static readonly Permission Update = new(53, "ROLES:UPDATE", "修改角色", PermissionScope.Tenant);
        public static readonly Permission Delete = new(54, "ROLES:DELETE", "刪除角色", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Delete
        };
    }
}
