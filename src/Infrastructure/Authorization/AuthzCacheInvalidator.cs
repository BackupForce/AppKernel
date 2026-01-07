using System.Net;
using Application.Abstractions.Authorization;
using Application.Abstractions.Caching;
using Domain.Security;
using StackExchange.Redis;

namespace Infrastructure.Authorization;

internal sealed class AuthzCacheInvalidator(
    ICacheService cacheService,
    IConnectionMultiplexer connectionMultiplexer)
    : IAuthzCacheInvalidator
{
    private const string RoleUsersPrefix = "authz:role-users:";
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public Task InvalidateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return cacheService.RemoveAsync(AuthzCacheKeys.ForUser(userId), cancellationToken);
    }

    public Task InvalidateUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        Task[] tasks = userIds
            .Distinct()
            .Select(userId => cacheService.RemoveAsync(AuthzCacheKeys.ForUser(userId), cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }

    public async Task InvalidateRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        RedisKey roleUsersKey = RoleUsersKey(roleId);
        RedisValue[] members = await _database.SetMembersAsync(roleUsersKey);

        if (members.Length == 0)
        {
            return;
        }

        var tasks = new List<Task>(members.Length);
        foreach (RedisValue member in members)
        {
            if (Guid.TryParse(member.ToString(), out Guid userId))
            {
                tasks.Add(cacheService.RemoveAsync(AuthzCacheKeys.ForUser(userId), cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
    }

    public Task InvalidateSubjectAsync(
        SubjectType subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default)
    {
        return subjectType switch
        {
            SubjectType.User => InvalidateUserAsync(subjectId, cancellationToken),
            SubjectType.Role => InvalidateRoleAsync(ExtractRoleId(subjectId), cancellationToken),
            SubjectType.Group => InvalidateAllMatricesAsync(cancellationToken),
            _ => InvalidateAllMatricesAsync(cancellationToken)
        };
    }

    public Task InvalidateAllMatricesAsync(CancellationToken cancellationToken = default)
    {
        return RemoveKeysByPatternAsync($"{AuthzCacheKeys.UserMatrixPrefix}*", cancellationToken);
    }

    public Task TrackRoleUserAsync(int roleId, Guid userId, CancellationToken cancellationToken = default)
    {
        return _database.SetAddAsync(RoleUsersKey(roleId), userId.ToString("D"));
    }

    public Task RemoveRoleIndexAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return _database.KeyDeleteAsync(RoleUsersKey(roleId));
    }

    private static int ExtractRoleId(Guid subjectId)
    {
        return BitConverter.ToInt32(subjectId.ToByteArray(), 0);
    }

    private async Task RemoveKeysByPatternAsync(string pattern, CancellationToken cancellationToken)
    {
        EndPoint[] endpoints = connectionMultiplexer.GetEndPoints();
        var tasks = new List<Task>();

        foreach (EndPoint endpoint in endpoints)
        {
            IServer server = connectionMultiplexer.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            foreach (RedisKey key in server.Keys(pattern: pattern))
            {
                tasks.Add(_database.KeyDeleteAsync(key));
            }
        }

        await Task.WhenAll(tasks);
    }

    private static RedisKey RoleUsersKey(int roleId) => $"{RoleUsersPrefix}{roleId}";
}
