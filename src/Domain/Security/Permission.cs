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
        public static readonly Permission ResetPassword = new(5, "USERS:RESET_PASSWORD", "重設密碼", PermissionScope.Tenant);

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
        public static readonly Permission View = new(11, "MEMBERS:VIEW", "檢視會員資料", PermissionScope.Tenant);
        public static readonly Permission Create = new(12, "MEMBERS:CREATE", "建立會員", PermissionScope.Tenant);
        public static readonly Permission Update = new(13, "MEMBERS:UPDATE", "修改會員資料", PermissionScope.Tenant);
        public static readonly Permission Suspend = new(14, "MEMBERS:SUSPEND", "停權會員", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Suspend
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
        public static readonly Permission DrawAll = new(203, "GAMING:DRAW:*", "期數所有權限", PermissionScope.Tenant);
        public static readonly Permission DrawGroupAll = new(204, "GAMING:DRAW_GROUP:*", "期數群組所有權限", PermissionScope.Tenant);
        public static readonly Permission TicketAll = new(205, "GAMING:TICKET:*", "票券所有權限", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventAll = new(206, "GAMING:TICKET_CLAIM_EVENT:*", "領券活動所有權限", PermissionScope.Tenant);
        public static readonly Permission WinningsAll = new(207, "GAMING:WINNINGS:*", "中獎兌獎所有權限", PermissionScope.Tenant);

        public static readonly Permission CatalogView = new(201, "GAMING:CATALOG:VIEW", "檢視遊戲目錄", PermissionScope.Tenant);
        public static readonly Permission EntitlementManage = new(202, "GAMING:ENTITLEMENT:MANAGE", "管理租戶遊戲啟用", PermissionScope.Tenant);

        public static readonly Permission DrawCreate = new(210, "GAMING:DRAW:CREATE", "建立期數", PermissionScope.Tenant);
        public static readonly Permission DrawExecute = new(211, "GAMING:DRAW:EXECUTE", "執行開獎", PermissionScope.Tenant);
        public static readonly Permission DrawSettle = new(212, "GAMING:DRAW:SETTLE", "結算開獎", PermissionScope.Tenant);
        public static readonly Permission DrawManualClose = new(213, "GAMING:DRAW:MANUAL_CLOSE", "手動封盤", PermissionScope.Tenant);
        public static readonly Permission DrawReopen = new(214, "GAMING:DRAW:REOPEN", "重新開盤", PermissionScope.Tenant);
        public static readonly Permission DrawUpdateAllowedTemplates = new(215, "GAMING:DRAW:UPDATE_ALLOWED_TEMPLATES", "更新期數允許票種", PermissionScope.Tenant);
        public static readonly Permission DrawTemplateManage = new(216, "GAMING:DRAW_TEMPLATE:MANAGE", "管理期數模板", PermissionScope.Tenant);
        public static readonly Permission DrawManage = new(217, "GAMING:DRAW:MANAGE", "管理期數", PermissionScope.Tenant);

        public static readonly Permission DrawGroupRead = new(220, "GAMING:DRAW_GROUP:READ", "檢視期數群組", PermissionScope.Tenant);
        public static readonly Permission DrawGroupCreate = new(221, "GAMING:DRAW_GROUP:CREATE", "建立期數群組", PermissionScope.Tenant);
        public static readonly Permission DrawGroupUpdate = new(222, "GAMING:DRAW_GROUP:UPDATE", "更新期數群組", PermissionScope.Tenant);
        public static readonly Permission DrawGroupActivate = new(223, "GAMING:DRAW_GROUP:ACTIVATE", "啟用期數群組", PermissionScope.Tenant);
        public static readonly Permission DrawGroupEnd = new(224, "GAMING:DRAW_GROUP:END", "結束期數群組", PermissionScope.Tenant);
        public static readonly Permission DrawGroupDelete = new(225, "GAMING:DRAW_GROUP:DELETE", "刪除期數群組", PermissionScope.Tenant);
        public static readonly Permission DrawGroupDrawManage = new(226, "GAMING:DRAW_GROUP_DRAW:MANAGE", "管理期數群組期數", PermissionScope.Tenant);
        public static readonly Permission DrawGroupManage = new(227, "GAMING:DRAW_GROUP:MANAGE", "管理期數群組", PermissionScope.Tenant);

        public static readonly Permission TicketRead = new(228, "GAMING:TICKET:READ", "後台查詢票券", PermissionScope.Tenant);
        public static readonly Permission TicketPlace = new(229, "GAMING:TICKET:PLACE", "後台代客下注", PermissionScope.Tenant);
        public static readonly Permission TicketCancel = new(241, "GAMING:TICKET:CANCEL", "後台取消票券", PermissionScope.Tenant);
        public static readonly Permission TicketManage = new(242, "GAMING:TICKET:MANAGE", "管理票券", PermissionScope.Tenant);

        public static readonly Permission TicketClaimEventRead = new(230, "GAMING:TICKET_CLAIM_EVENT:READ", "檢視領券活動", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventCreate = new(231, "GAMING:TICKET_CLAIM_EVENT:CREATE", "建立領券活動", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventUpdate = new(232, "GAMING:TICKET_CLAIM_EVENT:UPDATE", "更新領券活動", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventActivate = new(233, "GAMING:TICKET_CLAIM_EVENT:ACTIVATE", "啟用領券活動", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventDisable = new(234, "GAMING:TICKET_CLAIM_EVENT:DISABLE", "停用領券活動", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventEnd = new(235, "GAMING:TICKET_CLAIM_EVENT:END", "結束領券活動", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventClaimRead = new(236, "GAMING:TICKET_CLAIM_EVENT_CLAIM:READ", "檢視領券紀錄", PermissionScope.Tenant);
        public static readonly Permission TicketClaimEventManage = new(243, "GAMING:TICKET_CLAIM_EVENT:MANAGE", "管理領券活動", PermissionScope.Tenant);

        public static readonly Permission WinningNumbersRead = new(237, "GAMING:WINNING_NUMBERS:READ", "後台檢視開獎號碼", PermissionScope.Tenant);
        public static readonly Permission WinningsRead = new(238, "GAMING:WINNINGS:READ", "後台檢視中獎兌獎資料", PermissionScope.Tenant);
        public static readonly Permission WinningsRedeem = new(239, "GAMING:WINNINGS:REDEEM", "後台執行中獎兌獎", PermissionScope.Tenant);
        public static readonly Permission WinningRedeemedRead = new(240, "GAMING:WINNING_REDEEMED:READ", "後台依兌換時間檢視已兌獎資料", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All,
            DrawAll,
            DrawGroupAll,
            TicketAll,
            TicketClaimEventAll,
            WinningsAll,
            CatalogView,
            EntitlementManage,
            DrawCreate,
            DrawExecute,
            DrawSettle,
            DrawManualClose,
            DrawReopen,
            DrawUpdateAllowedTemplates,
            DrawTemplateManage,
            DrawManage,
            DrawGroupRead,
            DrawGroupCreate,
            DrawGroupUpdate,
            DrawGroupActivate,
            DrawGroupEnd,
            DrawGroupDelete,
            DrawGroupDrawManage,
            DrawGroupManage,
            TicketRead,
            TicketPlace,
            TicketCancel,
            TicketManage,
            TicketClaimEventRead,
            TicketClaimEventCreate,
            TicketClaimEventUpdate,
            TicketClaimEventActivate,
            TicketClaimEventDisable,
            TicketClaimEventEnd,
            TicketClaimEventClaimRead,
            TicketClaimEventManage,
            WinningNumbersRead,
            WinningsRead,
            WinningsRedeem,
            WinningRedeemedRead
        };
    }

    /// <summary>
    /// 票券後台操作權限定義（舊模組，保留以兼容既有代碼）
    /// </summary>
    public static class Tickets
    {
        public static readonly Permission All = new(260, "TICKETS:*", "票券後台所有權限", PermissionScope.Tenant);
        public static readonly Permission Issue = new(261, "TICKETS:ISSUE", "後台發放票券", PermissionScope.Tenant);
        public static readonly Permission PlaceBet = new(262, "TICKETS:PLACE_BET", "後台代客下注", PermissionScope.Tenant);
        public static readonly Permission Read = new(263, "TICKETS:READ", "後台查詢票券", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All,
            Issue,
            PlaceBet,
            Read
        };
    }

    public static Permission CreateForRole(string name, string description, int roleId)
    {
        // 中文註解：建立屬於角色的權限，描述預設為空字串以符合資料表不允許 null 的限制。
        string normalizedName = PermissionCatalog.NormalizeCode(name);
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
