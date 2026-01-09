using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class GroupRepository(ApplicationDbContext context) : IGroupRepository
{
    public Task<Group?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken)
    {
        return context.Set<Group>().FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludingGroupId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        string candidateName = name.Trim();
        string normalizedName = candidateName.ToUpperInvariant();

        IQueryable<Group> query = context.Set<Group>();

        if (excludingGroupId.HasValue)
        {
            Guid excludedId = excludingGroupId.Value;

            bool exists = await query.AnyAsync(
                group =>
                    group.Id != excludedId &&
                    group.Name != null &&
                    group.Name == normalizedName,
                cancellationToken);

            return !exists;
        }

        bool nameExists = await query.AnyAsync(
            group => group.Name != null && group.Name == normalizedName,
            cancellationToken);

        return !nameExists;
    }

    public void Insert(Group group)
    {
        context.Set<Group>().Add(group);
    }

    public void Remove(Group group)
    {
        context.Set<Group>().Remove(group);
    }
}
