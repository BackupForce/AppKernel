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
}
