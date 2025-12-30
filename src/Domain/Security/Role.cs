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

    public int Id { get; init; }

    public string Name { get; init; }

    public ICollection<User> Users { get; init; } = new List<User>();
    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}
