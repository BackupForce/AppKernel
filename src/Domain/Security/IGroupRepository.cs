namespace Domain.Security;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken);

    Task<bool> IsNameUniqueAsync(string name, Guid? excludingGroupId, CancellationToken cancellationToken);

    void Insert(Group group);

    void Remove(Group group);
}
