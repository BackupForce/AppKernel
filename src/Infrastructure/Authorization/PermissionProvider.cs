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

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, Guid? nodeId, Guid? tenantId)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        if (!tenantId.HasValue)
        {
            return false;
        }

        bool isInTenant = await _dbContext.UserTenants
            .AsNoTracking()
            .AnyAsync(userTenant => userTenant.UserId == userId && userTenant.TenantId == tenantId.Value);
        if (!isInTenant)
        {
            return false;
        }

        string cacheKey = AuthzCacheKeys.ForUserTenant(userId, tenantId);
        UserPermissionMatrix? cachedMatrix =
            await _cacheService.GetAsync<UserPermissionMatrix>(cacheKey);

        if (cachedMatrix is null)
        {
            cachedMatrix = await BuildUserPermissionMatrixAsync(userId);
            await _cacheService.SetAsync(cacheKey, cachedMatrix, CacheTtl);
        }

        return await EvaluatePermissionAsync(cachedMatrix, permissionCode, nodeId, tenantId);
    }

    private async Task<UserPermissionMatrix> BuildUserPermissionMatrixAsync(Guid userId)
    {
        List<int> roleIds = await _dbContext.Users
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Roles.Select(role => role.Id))
            .ToListAsync();

        var roleSubjectIds = roleIds
            .Select(MapRoleIdToSubjectId)
            .ToList();

        List<Guid> groupIds = await _dbContext.UserGroups
            .AsNoTracking()
            .Where(userGroup => userGroup.UserId == userId)
            .Select(userGroup => userGroup.GroupId)
            .ToListAsync();

        List<PermissionAssignment> assignments = await _dbContext.PermissionAssignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.SubjectType == SubjectType.User && assignment.SubjectId == userId
                || assignment.SubjectType == SubjectType.Role && roleSubjectIds.Contains(assignment.SubjectId)
                || assignment.SubjectType == SubjectType.Group && groupIds.Contains(assignment.SubjectId))
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
        Guid? nodeId,
        Guid? tenantId)
    {
        string normalizedCode = NormalizePermissionCode(permissionCode);
        if (!matrix.Decisions.TryGetValue(normalizedCode, out List<PermissionDecisionEntry>? decisions))
        {
            return false;
        }

        IReadOnlyList<Guid?> nodeScope = await GetNodeScopeAsync(nodeId, tenantId);
        if (nodeScope.Count == 0)
        {
            return false;
        }

        var nodeScopeSet = new HashSet<Guid?>(nodeScope);

        var relevantDecisions = decisions
            .Where(entry => nodeScopeSet.Contains(entry.NodeId))
            .ToList();

        if (relevantDecisions.Any(entry => entry.Decision == Decision.Deny))
        {
            return false;
        }

        return relevantDecisions.Any(entry => entry.Decision == Decision.Allow);
    }

    private async Task<IReadOnlyList<Guid?>> GetNodeScopeAsync(Guid? nodeId, Guid? tenantId)
    {
        if (!tenantId.HasValue)
        {
            return Array.Empty<Guid?>();
        }

        if (tenantId.HasValue && !nodeId.HasValue)
        {
            return new List<Guid?> { tenantId.Value };
        }

        if (!nodeId.HasValue)
        {
            return new List<Guid?> { null };
        }

        Guid currentNodeId = nodeId.Value;
        var lineage = new List<Guid?> { currentNodeId };
        var visited = new HashSet<Guid> { currentNodeId };
        Guid? currentId = currentNodeId;

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

        if (tenantId.HasValue)
        {
            if (!lineage.Contains(tenantId.Value))
            {
                return Array.Empty<Guid?>();
            }

            return lineage;
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
        return permissionCode.Trim().ToUpperInvariant();
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
