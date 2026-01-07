using Domain.Users;

namespace Domain.Security;

public sealed class UserGroup
{
    private UserGroup()
    {
    }

    private UserGroup(Guid userId, Guid groupId)
    {
        UserId = userId;
        GroupId = groupId;
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid GroupId { get; private set; }

    public Group Group { get; private set; } = null!;

    public static UserGroup Create(Guid userId, Guid groupId)
    {
        return new UserGroup(userId, groupId);
    }
}
