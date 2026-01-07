using Domain.Security;

namespace Application.Abstractions.Authorization;

public interface IAuthzCacheInvalidator
{
    Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task InvalidateUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    Task InvalidateRoleAsync(int roleId, CancellationToken cancellationToken = default);

    Task InvalidateSubjectAsync(
        SubjectType subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default);

    Task InvalidateAllMatricesAsync(CancellationToken cancellationToken = default);

    Task TrackRoleUserAsync(int roleId, Guid userId, CancellationToken cancellationToken = default);

    Task RemoveRoleIndexAsync(int roleId, CancellationToken cancellationToken = default);

    Task TrackGroupUserAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);

    Task UntrackGroupUserAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);

    Task RemoveGroupIndexAsync(Guid groupId, CancellationToken cancellationToken = default);
}
