namespace Domain.Security;
public sealed class Permission
{
    private Permission(int id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public int Id { get; init; }

    public string Name { get; init; }
    public string Description { get; init; }

    /// <summary>
    /// 針對使用者列表UserList的權限定義
    /// </summary>
    public static class Users
    {
        public static readonly Permission All = new(0, "users:*", "使用者模組所有權限");
        public static readonly Permission View = new(1, "users:view", "檢視使用者資料");
        public static readonly Permission Create = new(2, "users:create", "建立使用者");
        public static readonly Permission Update = new(3, "users:update", "修改使用者");
        public static readonly Permission Delete = new(4, "users:delete", "刪除使用者");
        public static readonly Permission ResetPassword = new(5, "users:reset-password", "重設密碼");

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
        public static readonly Permission All = new(10, "members:*", "會員模組所有權限");
        public static readonly Permission Read = new(11, "members:read", "檢視會員資料");
        public static readonly Permission Create = new(12, "members:create", "建立會員");
        public static readonly Permission Update = new(13, "members:update", "修改會員資料");
        public static readonly Permission Suspend = new(14, "members:suspend", "停權會員");

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
        public static readonly Permission All = new(20, "member_points:*", "會員點數所有權限");
        public static readonly Permission Read = new(21, "member_points:read", "檢視會員點數");
        public static readonly Permission Adjust = new(22, "member_points:adjust", "人工調整會員點數");
        public static readonly Permission Transfer = new(23, "member_points:transfer", "會員點數轉帳");

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
        public static readonly Permission All = new(30, "member_assets:*", "會員資產所有權限");
        public static readonly Permission Read = new(31, "member_assets:read", "檢視會員資產");
        public static readonly Permission Adjust = new(32, "member_assets:adjust", "調整會員資產");

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
        public static readonly Permission All = new(40, "member_audit:*", "會員操作歷程所有權限");
        public static readonly Permission Read = new(41, "member_audit:read", "檢視會員操作歷程");

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read
        };
    }
}
