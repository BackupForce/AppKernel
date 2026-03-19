namespace Domain.Security;

public sealed partial class Permission
{
    /// <summary>
    /// 會員標籤權限定義
    /// </summary>
    public static class MemberTag
    {
        public static readonly Permission All = new(15, "MEMBER_TAG:*", "會員標籤所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(16, "MEMBER_TAG:READ", "檢視會員標籤", PermissionScope.Tenant);
        public static readonly Permission Create = new(17, "MEMBER_TAG:CREATE", "建立會員標籤", PermissionScope.Tenant);
        public static readonly Permission Update = new(18, "MEMBER_TAG:UPDATE", "更新會員標籤", PermissionScope.Tenant);
        public static readonly Permission Delete = new(19, "MEMBER_TAG:DELETE", "刪除會員標籤", PermissionScope.Tenant);
        public static readonly Permission Assign = new(24, "MEMBER_TAG:ASSIGN", "指派標籤給會員", PermissionScope.Tenant);
        public static readonly Permission Unassign = new(25, "MEMBER_TAG:UNASSIGN", "移除會員標籤", PermissionScope.Tenant);
        public static readonly Permission Activate = new(26, "MEMBER_TAG:ACTIVATE", "啟用會員標籤", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Create, Update, Delete, Assign, Unassign, Activate
        };
    }
}
