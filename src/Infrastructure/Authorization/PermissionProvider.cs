using Application.Abstractions.Authorization;
using Application.Abstractions.Caching;
using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider : IPermissionProvider
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(20);
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;

    public PermissionProvider(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, Guid? nodeId)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        string cacheKey = $"authz:permissions:{userId}";
        UserPermissionMatrix? cachedMatrix =
            await _cacheService.GetAsync<UserPermissionMatrix>(cacheKey);

        if (cachedMatrix is null)
        {
            cachedMatrix = await BuildUserPermissionMatrixAsync(userId);
            await _cacheService.SetAsync(cacheKey, cachedMatrix, CacheTtl);
        }

        return await EvaluatePermissionAsync(cachedMatrix, permissionCode, nodeId);
    }

    private async Task<UserPermissionMatrix> BuildUserPermissionMatrixAsync(Guid userId)
    {
        List<int> roleIds = await _dbContext.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Roles.Select(role => role.Id))
            .ToListAsync();

        List<Guid> roleSubjectIds = roleIds
            .Select(MapRoleIdToSubjectId)
            .ToList();

        List<Guid> groupIds = new();

        List<PermissionAssignment> assignments = await _dbContext.PermissionAssignments
            .AsNoTracking()
            .Where(assignment =>
                (assignment.SubjectType == SubjectType.User && assignment.SubjectId == userId)
                || (assignment.SubjectType == SubjectType.Role && roleSubjectIds.Contains(assignment.SubjectId))
                || (assignment.SubjectType == SubjectType.Group && groupIds.Contains(assignment.SubjectId)))
            .ToListAsync();

        List<string> rolePermissionCodes = await _dbContext.Set<Permission>()
            .AsNoTracking()
            .Where(permission => permission.RoleId.HasValue && roleIds.Contains(permission.RoleId.Value))
            .Select(permission => permission.Name)
            .ToListAsync();

        var matrix = new UserPermissionMatrix();

        foreach (PermissionAssignment assignment in assignments)
        {
            AddDecision(matrix, assignment.PermissionCode, assignment.NodeId, assignment.Decision);
        }

        foreach (string permissionCode in rolePermissionCodes)
        {
            AddDecision(matrix, permissionCode, null, Decision.Allow);
        }

        return matrix;
    }

    private async Task<bool> EvaluatePermissionAsync(
        UserPermissionMatrix matrix,
        string permissionCode,
        Guid? nodeId)
    {
        string normalizedCode = NormalizePermissionCode(permissionCode);
        if (!matrix.Decisions.TryGetValue(normalizedCode, out List<PermissionDecisionEntry>? decisions))
        {
            return false;
        }

        IEnumerable<Guid?> nodeScope = nodeId.HasValue
            ? await GetNodeScopeAsync(nodeId.Value)
            : new Guid?[] { null };

        var nodeScopeSet = new HashSet<Guid?>(nodeScope);

        List<PermissionDecisionEntry> relevantDecisions = decisions
            .Where(entry => nodeScopeSet.Contains(entry.NodeId))
            .ToList();

        if (relevantDecisions.Any(entry => entry.Decision == Decision.Deny))
        {
            return false;
        }

        return relevantDecisions.Any(entry => entry.Decision == Decision.Allow);
    }

    private async Task<IReadOnlyList<Guid?>> GetNodeScopeAsync(Guid nodeId)
    {
        var lineage = new List<Guid?> { nodeId };
        var visited = new HashSet<Guid> { nodeId };
        Guid? currentId = nodeId;

        while (currentId.HasValue)
        {
            ResourceNodeParent? node = await _dbContext.ResourceNodes
                .AsNoTracking()
                .Where(resourceNode => resourceNode.Id == currentId.Value)
                .Select(resourceNode => new ResourceNodeParent(resourceNode.Id, resourceNode.ParentId))
                .SingleOrDefaultAsync();

            if (node is null || node.ParentId is null)
            {
                break;
            }

            if (!visited.Add(node.ParentId.Value))
            {
                break;
            }

            lineage.Add(node.ParentId);
            currentId = node.ParentId;
        }

        lineage.Add(null);

        return lineage;
    }

    private static void AddDecision(
        UserPermissionMatrix matrix,
        string permissionCode,
        Guid? nodeId,
        Decision decision)
    {
        string normalizedCode = NormalizePermissionCode(permissionCode);
        if (!matrix.Decisions.TryGetValue(normalizedCode, out List<PermissionDecisionEntry>? entries))
        {
            entries = new List<PermissionDecisionEntry>();
            matrix.Decisions[normalizedCode] = entries;
        }

        PermissionDecisionEntry? existing = entries.FirstOrDefault(entry => entry.NodeId == nodeId);
        if (existing is not null)
        {
            if (existing.Decision == Decision.Deny)
            {
                return;
            }

            if (decision == Decision.Deny)
            {
                entries.Remove(existing);
                entries.Add(new PermissionDecisionEntry(nodeId, decision));
            }

            return;
        }

        entries.Add(new PermissionDecisionEntry(nodeId, decision));
    }

    private static string NormalizePermissionCode(string permissionCode)
    {
        return permissionCode.Trim().ToLowerInvariant();
    }

    private static Guid MapRoleIdToSubjectId(int roleId)
    {
        return new Guid(roleId, 0, 0, new byte[8]);
    }

    private sealed record PermissionDecisionEntry(Guid? NodeId, Decision Decision);

    private sealed class UserPermissionMatrix
    {
        public Dictionary<string, List<PermissionDecisionEntry>> Decisions { get; init; } = new();
    }

    private sealed record ResourceNodeParent(Guid Id, Guid? ParentId);
}
