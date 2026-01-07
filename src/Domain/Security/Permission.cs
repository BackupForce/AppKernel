namespace Domain.Security;
public sealed class Permission
{
    private Permission(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    private Permission()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public int? RoleId { get; set; }

    /// <summary>
    /// 針對使用者列表UserList的權限定義
    /// </summary>
    public static class Users
    {
        public static readonly Permission All = new(0, "USERS:*", "使用者模組所有權限");
        public static readonly Permission View = new(1, "USERS:VIEW", "檢視使用者資料");
        public static readonly Permission Create = new(2, "USERS:CREATE", "建立使用者");
        public static readonly Permission Update = new(3, "USERS:UPDATE", "修改使用者");
        public static readonly Permission Delete = new(4, "USERS:DELETE", "刪除使用者");
        public static readonly Permission ResetPassword = new(5, "USERS:RESET-PASSWORD", "重設密碼");

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
        public static readonly Permission All = new(10, "MEMBERS:*", "會員模組所有權限");
        public static readonly Permission Read = new(11, "MEMBERS:READ", "檢視會員資料");
        public static readonly Permission Create = new(12, "MEMBERS:CREATE", "建立會員");
        public static readonly Permission Update = new(13, "MEMBERS:UPDATE", "修改會員資料");
        public static readonly Permission Suspend = new(14, "MEMBERS:SUSPEND", "停權會員");

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Create, Update, Suspend
        };
    }

    /// <summary>
    /// 會員點數權限定義
    /// </summary>
    public static class MemberPoints
    {
        public static readonly Permission All = new(20, "MEMBER_POINTS:*", "會員點數所有權限");
        public static readonly Permission Read = new(21, "MEMBER_POINTS:READ", "檢視會員點數");
        public static readonly Permission Adjust = new(22, "MEMBER_POINTS:ADJUST", "人工調整會員點數");
        public static readonly Permission Transfer = new(23, "MEMBER_POINTS:TRANSFER", "會員點數轉帳");

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
        public static readonly Permission All = new(30, "MEMBER_ASSETS:*", "會員資產所有權限");
        public static readonly Permission Read = new(31, "MEMBER_ASSETS:READ", "檢視會員資產");
        public static readonly Permission Adjust = new(32, "MEMBER_ASSETS:ADJUST", "調整會員資產");

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
        public static readonly Permission All = new(40, "MEMBER_AUDIT:*", "會員操作歷程所有權限");
        public static readonly Permission Read = new(41, "MEMBER_AUDIT:READ", "檢視會員操作歷程");

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
        public static readonly Permission All = new(50, "ROLES:*", "角色模組所有權限");
        public static readonly Permission View = new(51, "ROLES:VIEW", "檢視角色");
        public static readonly Permission Create = new(52, "ROLES:CREATE", "建立角色");
        public static readonly Permission Update = new(53, "ROLES:UPDATE", "修改角色");
        public static readonly Permission Delete = new(54, "ROLES:DELETE", "刪除角色");

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, View, Create, Update, Delete
        };
    }

    public static Permission CreateForRole(string name, string description, int roleId)
    {
        // 中文註解：建立屬於角色的權限，描述預設為空字串以符合資料表不允許 null 的限制。
        return new Permission
        {
            Name = name,
            Description = description,
            RoleId = roleId
        };
    }
}
