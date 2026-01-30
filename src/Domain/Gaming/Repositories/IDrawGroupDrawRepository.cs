using Domain.Gaming.DrawGroups;

namespace Domain.Gaming.Repositories;

public interface IDrawGroupDrawRepository
{
    Task<IReadOnlyCollection<DrawGroupDraw>> GetByDrawGroupIdAsync(
        Guid tenantId,
        Guid drawGroupId,
        CancellationToken cancellationToken = default);
}
