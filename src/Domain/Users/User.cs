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

    public Email Email { get; private set; }

    public Name Name { get; private set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool HasPublicProfile { get; set; }

    public static User Create(Email email, Name name,string passwordhash, bool hasPublicProfile)
    {
        var user = new User(Guid.NewGuid(), email, name, passwordhash, hasPublicProfile);

        user.Raise(new UserCreatedDomainEvent(user.Id));
        //寫入初始Role

        return user;
    }
}
