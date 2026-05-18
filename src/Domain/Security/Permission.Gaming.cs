namespace Domain.Security;

public sealed partial class Permission
{
    /// <summary>
    /// 遊戲主領域權限定義
    /// </summary>
    public static class Gaming
    {
        public static readonly Permission All = new(200, "GAMING:*", "遊戲模組所有權限", PermissionScope.Tenant);
        public static readonly Permission CatalogView = new(201, "GAMING:CATALOG:READ", "檢視遊戲目錄", PermissionScope.Tenant);
        public static readonly Permission EntitlementManage = new(202, "GAMING:ENTITLEMENT:MANAGE", "管理租戶遊戲啟用", PermissionScope.Tenant);
        public static readonly Permission DrawTemplateManage = new(216, "GAMING:DRAW_TEMPLATE:MANAGE", "管理期數模板", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All,
            CatalogView,
            EntitlementManage,
            DrawTemplateManage
        };
    }
}
