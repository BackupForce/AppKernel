using Domain.Security;
using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity
{
    private User(
        Guid id,
        Email email,
        Name name,
        string passwordhash,
        bool hasPublicProfile,
        UserType type,
        Guid? tenantId)
        : base(id)
    {
        Email = email;
        Name = name;
        HasPublicProfile = hasPublicProfile;
        PasswordHash = passwordhash;
        Type = type;
        TenantId = tenantId;
        NormalizedEmail = NormalizeForLookup(email.Value);

        EnsureTypeInvariant(type, tenantId);
    }

    private User()
    {
    }

    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.ToList();

    private readonly List<UserGroup> _userGroups = new();
    public IReadOnlyCollection<UserGroup> UserGroups => _userGroups.ToList();

    private readonly List<LoginBinding> _loginBindings = new();
    public IReadOnlyCollection<LoginBinding> LoginBindings => _loginBindings.ToList();

    public Email Email { get; private set; }

    public string NormalizedEmail { get; private set; } = string.Empty;

    public Name Name { get; private set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool HasPublicProfile { get; set; }

    public UserType Type { get; private set; }

    public Guid? TenantId { get; private set; }

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

    public static User Create(
        Email email,
        Name name,
        string passwordhash,
        bool hasPublicProfile,
        UserType type,
        Guid? tenantId)
    {
        // 中文註解：建立使用者時先檢查型別與租戶不變式，避免建立非法資料。
        EnsureTypeInvariant(type, tenantId);

        User user = new User(Guid.NewGuid(), email, name, passwordhash, hasPublicProfile, type, tenantId);

        user.Raise(new UserCreatedDomainEvent(user.Id));
        //寫入初始Role

        return user;
    }

    public void UpdateType(UserType type, Guid? tenantId)
    {
        // 中文註解：變更使用者型別時也必須遵守租戶不變式，避免權限混亂。
        EnsureTypeInvariant(type, tenantId);

        Type = type;
        TenantId = tenantId;

        foreach (LoginBinding binding in _loginBindings)
        {
            binding.UpdateTenantId(tenantId);
        }
    }

    public bool IsPlatform()
    {
        return Type == UserType.Platform;
    }

    public bool IsTenantUser()
    {
        return Type == UserType.Tenant;
    }

    public bool IsMember()
    {
        return Type == UserType.Member;
    }

    public Guid RequireTenantId()
    {
        // 中文註解：租戶/會員使用者必須綁定 TenantId，缺失就直接阻擋。
        if (!TenantId.HasValue)
        {
            throw new InvalidOperationException("User TenantId is required but missing.");
        }

        return TenantId.Value;
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

        UserGroup? target = _userGroups.Find(userGroup => userGroup.GroupId == group.Id);
        if (target is null)
        {
            return;
        }

        _userGroups.Remove(target);
    }

    private static void EnsureTypeInvariant(UserType type, Guid? tenantId)
    {
        // 中文註解：Platform 必須沒有 TenantId，Tenant/Member 必須有 TenantId。
        if (type == UserType.Platform && tenantId.HasValue)
        {
            throw new InvalidOperationException("Platform user cannot have tenant id.");
        }

        if (type != UserType.Platform && !tenantId.HasValue)
        {
            throw new InvalidOperationException("Tenant/Member user must have tenant id.");
        }
    }

    public Result BindLogin(LoginProvider provider, string providerKey, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            return Result.Failure(UserErrors.LoginProviderKeyRequired);
        }

        LoginBinding? existing = _loginBindings.FirstOrDefault(binding => binding.Provider == provider);
        if (existing is not null)
        {
            string normalizedKey = LoginBinding.Normalize(provider, providerKey);
            if (existing.NormalizedProviderKey == normalizedKey)
            {
                return Result.Success();
            }

            return Result.Failure(UserErrors.LoginProviderAlreadyBound(provider));
        }

        Result<LoginBinding> creationResult = LoginBinding.Create(provider, providerKey, Id, TenantId, utcNow);
        if (creationResult.IsFailure)
        {
            return Result.Failure(creationResult.Error);
        }

        _loginBindings.Add(creationResult.Value);
        return Result.Success();
    }

    public Result UnbindLogin(LoginProvider provider)
    {
        LoginBinding? existing = _loginBindings.FirstOrDefault(binding => binding.Provider == provider);
        if (existing is null)
        {
            return Result.Failure(UserErrors.LoginProviderNotBound(provider));
        }

        if (_loginBindings.Count == 1)
        {
            return Result.Failure(UserErrors.LoginBindingRequired);
        }

        _loginBindings.Remove(existing);
        return Result.Success();
    }

    public bool HasLogin(LoginProvider provider)
    {
        return _loginBindings.Any(binding => binding.Provider == provider);
    }

    public LoginBinding? GetLogin(LoginProvider provider)
    {
        return _loginBindings.FirstOrDefault(binding => binding.Provider == provider);
    }

    private static string NormalizeForLookup(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
