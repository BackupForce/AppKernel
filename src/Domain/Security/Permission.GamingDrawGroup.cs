namespace Domain.Security;

public sealed partial class Permission
{
    public static class GamingDrawGroup
    {
        public static readonly Permission All = new(204, "GAMING:DRAW_GROUP:*", "期數群組所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(220, "GAMING:DRAW_GROUP:READ", "檢視期數群組", PermissionScope.Tenant);
        public static readonly Permission Create = new(221, "GAMING:DRAW_GROUP:CREATE", "建立期數群組", PermissionScope.Tenant);
        public static readonly Permission Update = new(222, "GAMING:DRAW_GROUP:UPDATE", "更新期數群組", PermissionScope.Tenant);
        public static readonly Permission Activate = new(223, "GAMING:DRAW_GROUP:ACTIVATE", "啟用期數群組", PermissionScope.Tenant);
        public static readonly Permission End = new(224, "GAMING:DRAW_GROUP:END", "結束期數群組", PermissionScope.Tenant);
        public static readonly Permission Delete = new(225, "GAMING:DRAW_GROUP:DELETE", "刪除期數群組", PermissionScope.Tenant);
        public static readonly Permission ManageDraw = new(226, "GAMING:DRAW_GROUP:MANAGE_DRAW", "管理期數群組期數", PermissionScope.Tenant);
        public static readonly Permission Manage = new(227, "GAMING:DRAW_GROUP:MANAGE", "管理期數群組", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Create, Update, Activate, End, Delete, ManageDraw, Manage
        };
    }
}
