using Domain.Security;
using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity
{
    private User(Guid id, Email email, Name name, string passwordhash, bool hasPublicProfile)
        : base(id)
    {
        Email = email;
        Name = name;
        HasPublicProfile = hasPublicProfile;
        PasswordHash = passwordhash;
    }

    private User()
    {
    }

    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    private readonly List<UserGroup> _userGroups = new();
    public IReadOnlyCollection<UserGroup> UserGroups => _userGroups.ToList();

    public Email Email { get; private set; }

    public Name Name { get; private set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool HasPublicProfile { get; set; }

    public bool HasRole(int roleId)
    {
        // 中文註解：檢查使用者是否已經擁有指定角色。
        return _roles.Any(role => role.Id == roleId);
    }

    public void AssignRole(Role role)
    {
        // 中文註解：將角色指派給使用者，避免重複新增。
        if (role is null)
        {
            return;
        }

        if (HasRole(role.Id))
        {
            return;
        }

        _roles.Add(role);
    }

    public static User Create(Email email, Name name, string passwordhash, bool hasPublicProfile)
    {
        var user = new User(Guid.NewGuid(), email, name, passwordhash, hasPublicProfile);

        user.Raise(new UserCreatedDomainEvent(user.Id));
        //寫入初始Role

        return user;
    }

    public bool HasGroup(Guid groupId)
    {
        return _userGroups.Any(userGroup => userGroup.GroupId == groupId);
    }

    public void AssignGroup(Group group)
    {
        if (group is null)
        {
            return;
        }

        if (HasGroup(group.Id))
        {
            return;
        }

        _userGroups.Add(UserGroup.Create(Id, group.Id));
    }

    public void RemoveGroup(Group group)
    {
        if (group is null)
        {
            return;
        }

        UserGroup? target = _userGroups.FirstOrDefault(userGroup => userGroup.GroupId == group.Id);
        if (target is null)
        {
            return;
        }

        _userGroups.Remove(target);
    }
}
