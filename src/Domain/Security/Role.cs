using Domain.Users;

namespace Domain.Security;
public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");//沒用
    public static readonly Role Owner = new(2, "Owner");
    public static readonly Role Operator = new(3, "Operator");
    public static readonly Role System = new(4, "System");
    public static readonly Role Agent = new(5, "Agent");
    public static readonly Role Member = new(6, "Member");

    private Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    private Role()
    {
        Name = string.Empty;
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public Guid? TenantId { get; private set; }

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    public static Role Create(string name, Guid? tenantId = null)
    {
        // 中文註解：平台角色不可綁 TenantId，租戶角色必須綁 TenantId，避免跨租戶誤用。
        EnsureTenantInvariant(tenantId);

        // 中文註解：建立可持久化的新角色，交由資料庫產生識別碼。
        return new Role
        {
            Name = name,
            TenantId = tenantId
        };
    }

    public void Rename(string name)
    {
        // 中文註解：更新角色名稱。
        Name = name;
    }

    public bool IsPlatformRole()
    {
        return !TenantId.HasValue;
    }

    public bool IsTenantRole()
    {
        return TenantId.HasValue;
    }

    private static void EnsureTenantInvariant(Guid? tenantId)
    {
        // 中文註解：平台角色必須為 null，租戶角色必須有值。
        if (!tenantId.HasValue)
        {
            return;
        }

        if (tenantId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant role requires a valid tenant id.");
        }
    }
}
