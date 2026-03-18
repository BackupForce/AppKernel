namespace Domain.Security;

public sealed partial class Permission
{
    public static class GamingDraw
    {
        public static readonly Permission All = new(203, "GAMING_DRAW:*", "期數所有權限", PermissionScope.Tenant);
        public static readonly Permission Create = new(210, "GAMING_DRAW:CREATE", "建立期數", PermissionScope.Tenant);
        public static readonly Permission Execute = new(211, "GAMING_DRAW:EXECUTE", "執行開獎", PermissionScope.Tenant);
        public static readonly Permission Settle = new(212, "GAMING_DRAW:SETTLE", "結算開獎", PermissionScope.Tenant);
        public static readonly Permission ManualClose = new(213, "GAMING_DRAW:MANUAL_CLOSE", "手動封盤", PermissionScope.Tenant);
        public static readonly Permission Reopen = new(214, "GAMING_DRAW:REOPEN", "重新開盤", PermissionScope.Tenant);
        public static readonly Permission UpdateAllowedTemplates = new(215, "GAMING_DRAW:UPDATE_ALLOWED_TEMPLATES", "更新期數允許票種", PermissionScope.Tenant);
        public static readonly Permission Manage = new(217, "GAMING_DRAW:MANAGE", "管理期數", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Create, Execute, Settle, ManualClose, Reopen, UpdateAllowedTemplates, Manage
        };
    }
}
