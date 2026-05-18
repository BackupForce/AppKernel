using Domain.Gaming.DrawGroups;

namespace Domain.Gaming.Repositories;

public interface IDrawGroupRepository
{
    Task<DrawGroup?> GetByIdAsync(Guid tenantId, Guid drawGroupId, CancellationToken cancellationToken = default);

    void Insert(DrawGroup drawGroup);

    void Update(DrawGroup drawGroup);

    void Remove(DrawGroup drawGroup);
}
