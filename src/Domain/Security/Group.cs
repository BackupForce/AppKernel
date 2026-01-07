using SharedKernel;

namespace Domain.Security;

public sealed class Group : Entity
{
    private Group(Guid id, string name, string externalKey)
        : base(id)
    {
        Name = name;
        ExternalKey = externalKey;
    }

    private Group()
    {
    }

    private readonly List<UserGroup> _userGroups = new();

    public IReadOnlyCollection<UserGroup> UserGroups => _userGroups.ToList();

    public string Name { get; private set; } = string.Empty;

    public string ExternalKey { get; private set; } = string.Empty;

    public static Group Create(string name, string externalKey)
    {
        return new Group(Guid.NewGuid(), name, externalKey);
    }
}
