namespace Domain.Security;
public sealed class Permission
{
    private Permission(int id, string name, string description, PermissionScope scope)
    {
        Id = id;
        Name = name;
        Description = description;
        Scope = scope;
    }

    private Permission()
    {
        Name = string.Empty;
        Description = string.Empty;
        Scope = PermissionScope.Tenant;
    }

    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public PermissionScope Scope { get; set; }
    public int? RoleId { get; set; }

    /// <summary>
    /// 針對使用者列表UserList的權限定義
    /// </summary>
    public static class Users
    {
        public static readonly Permission All = new(0, "USERS:*", "使用者模組所有權限", PermissionScope.Tenant);
        public static readonly Permission View = new(1, "USERS:VIEW", "檢視使用者資料", PermissionScope.Tenant);
        public static readonly Permission Create = new(2, "USERS:CREATE", "建立使用者", PermissionScope.Tenant);
        public static readonly Permission Update = new(3, "USERS:UPDATE", "修改使用者", PermissionScope.Tenant);
        public static readonly Permission Delete = new(4, "USERS:DELETE", "刪除使用者", PermissionScope.Tenant);
        public static readonly Permission ResetPassword = new(5, "USERS:RESET-PASSWORD", "重設密碼", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Delete, ResetPassword
        };

        public static IEnumerable<Permission> ChildPermissions => AllPermissions.Where(p => p != All);
    }

    /// <summary>
    /// 會員模組權限定義
    /// </summary>
    public static class Members
    {
        public static readonly Permission All = new(10, "MEMBERS:*", "會員模組所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(11, "MEMBERS:READ", "檢視會員資料", PermissionScope.Tenant);
        public static readonly Permission Create = new(12, "MEMBERS:CREATE", "建立會員", PermissionScope.Tenant);
        public static readonly Permission Update = new(13, "MEMBERS:UPDATE", "修改會員資料", PermissionScope.Tenant);
        public static readonly Permission Suspend = new(14, "MEMBERS:SUSPEND", "停權會員", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Create, Update, Suspend
        };
    }

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

    /// <summary>
    /// 個人點數權限定義（依租戶授權，Member 不走 RBAC）
    /// </summary>
    public static class Points
    {
        public static readonly Permission All = new(70, "POINTS:ME:*", "自身點數所有權限", PermissionScope.Tenant);
        public static readonly Permission View = new(71, "POINTS:ME:VIEW", "檢視自身點數", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View
        };
    }

    /// <summary>
    /// 會員點數權限定義
    /// </summary>
    public static class MemberPoints
    {
        public static readonly Permission All = new(20, "MEMBER_POINTS:*", "會員點數所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(21, "MEMBER_POINTS:READ", "檢視會員點數", PermissionScope.Tenant);
        public static readonly Permission Adjust = new(22, "MEMBER_POINTS:ADJUST", "人工調整會員點數", PermissionScope.Tenant);
        public static readonly Permission Transfer = new(23, "MEMBER_POINTS:TRANSFER", "會員點數轉帳", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Adjust, Transfer
        };
    }

    /// <summary>
    /// 會員資產權限定義
    /// </summary>
    public static class MemberAssets
    {
        public static readonly Permission All = new(30, "MEMBER_ASSETS:*", "會員資產所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(31, "MEMBER_ASSETS:READ", "檢視會員資產", PermissionScope.Tenant);
        public static readonly Permission Adjust = new(32, "MEMBER_ASSETS:ADJUST", "調整會員資產", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Adjust
        };
    }

    /// <summary>
    /// 會員操作歷程權限定義
    /// </summary>
    public static class MemberAudit
    {
        public static readonly Permission All = new(40, "MEMBER_AUDIT:*", "會員操作歷程所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(41, "MEMBER_AUDIT:READ", "檢視會員操作歷程", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read
        };
    }

    /// <summary>
    /// 角色管理權限定義
    /// </summary>
    public static class Roles
    {
        public static readonly Permission All = new(50, "ROLES:*", "角色模組所有權限", PermissionScope.Tenant);
        public static readonly Permission View = new(51, "ROLES:VIEW", "檢視角色", PermissionScope.Tenant);
        public static readonly Permission Create = new(52, "ROLES:CREATE", "建立角色", PermissionScope.Tenant);
        public static readonly Permission Update = new(53, "ROLES:UPDATE", "修改角色", PermissionScope.Tenant);
        public static readonly Permission Delete = new(54, "ROLES:DELETE", "刪除角色", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Delete
        };
    }

    /// <summary>
    /// 遊戲模組權限定義
    /// </summary>
    public static class Gaming
    {
        public static readonly Permission All = new(200, "GAMING:*", "遊戲模組所有權限", PermissionScope.Tenant);
        public static readonly Permission CatalogView = new(201, "GAMING:CATALOG:VIEW", "檢視遊戲目錄", PermissionScope.Tenant);
        public static readonly Permission EntitlementManage = new(202, "GAMING:ENTITLEMENT:MANAGE", "管理租戶遊戲啟用", PermissionScope.Tenant);
        public static readonly Permission DrawCreate = new(210, "GAMING:DRAW:CREATE", "建立期數", PermissionScope.Tenant);
        public static readonly Permission DrawExecute = new(211, "GAMING:DRAW:EXECUTE", "執行開獎", PermissionScope.Tenant);
        public static readonly Permission DrawSettle = new(212, "GAMING:DRAW:SETTLE", "結算開獎", PermissionScope.Tenant);
        public static readonly Permission DrawManualClose = new(213, "GAMING:DRAW:MANUAL-CLOSE", "手動封盤", PermissionScope.Tenant);
        public static readonly Permission DrawReopen = new(214, "GAMING:DRAW:REOPEN", "重新開盤", PermissionScope.Tenant);
        public static readonly Permission DrawUpdateAllowedTemplates = new(215, "GAMING:DRAW:UPDATE-ALLOWED-TEMPLATES", "更新期數允許票種", PermissionScope.Tenant);
        public static readonly Permission CampaignRead = new(220, "gaming.campaign.read", "檢視活動", PermissionScope.Tenant);
        public static readonly Permission CampaignCreate = new(221, "gaming.campaign.create", "建立活動", PermissionScope.Tenant);
        public static readonly Permission CampaignUpdate = new(222, "gaming.campaign.update", "更新活動", PermissionScope.Tenant);
        public static readonly Permission CampaignActivate = new(223, "gaming.campaign.activate", "啟用活動", PermissionScope.Tenant);
        public static readonly Permission CampaignEnd = new(224, "gaming.campaign.end", "結束活動", PermissionScope.Tenant);
        public static readonly Permission CampaignDelete = new(225, "gaming.campaign.delete", "刪除活動", PermissionScope.Tenant);
        public static readonly Permission CampaignDrawManage = new(226, "gaming.campaign.draw.manage", "管理活動期數", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All,
            CatalogView,
            EntitlementManage,
            DrawCreate,
            DrawExecute,
            DrawSettle,
            DrawManualClose,
            DrawReopen,
            DrawUpdateAllowedTemplates,
            CampaignRead,
            CampaignCreate,
            CampaignUpdate,
            CampaignActivate,
            CampaignEnd,
            CampaignDelete,
            CampaignDrawManage
        };
    }

    /// <summary>
    /// 票券後台操作權限定義
    /// </summary>
    public static class Tickets
    {
        public static readonly Permission All = new(260, "tickets:*", "票券後台所有權限", PermissionScope.Tenant);
        public static readonly Permission Issue = new(261, "tickets.issue", "後台發放票券", PermissionScope.Tenant);
        public static readonly Permission PlaceBet = new(262, "tickets.placeBet", "後台代客下注", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All,
            Issue,
            PlaceBet
        };
    }

    public static Permission CreateForRole(string name, string description, int roleId)
    {
        // 中文註解：建立屬於角色的權限，描述預設為空字串以符合資料表不允許 null 的限制。
        string normalizedName = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim().ToUpperInvariant();
        string normalizedDescription = description ?? string.Empty;
        PermissionScope scope = PermissionCatalog.ResolveScope(normalizedName);
        return new Permission
        {
            Name = normalizedName,
            Description = normalizedDescription,
            RoleId = roleId,
            Scope = scope
        };
    }
}
