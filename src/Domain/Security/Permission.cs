namespace Domain.Security;
public sealed partial class Permission
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
