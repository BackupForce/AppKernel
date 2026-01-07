using System;
using Application.Abstractions.Infrastructure;
using Domain.Security;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeders;

public sealed class MemberResourceNodeSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<MemberResourceNodeSeeder> _logger;

    public MemberResourceNodeSeeder(ApplicationDbContext db, ILogger<MemberResourceNodeSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        Dictionary<Guid, Guid?> rootNodeLookup = await _db.ResourceNodes
            .AsNoTracking()
            .Where(node => node.ParentId == null)
            .ToDictionaryAsync(node => node.TenantId, node => (Guid?)node.Id);

        var members = await _db.Members
            .AsNoTracking()
            .Select(member => new
            {
                member.Id,
                member.DisplayName,
                member.TenantId
            })
            .ToListAsync();

        if (members.Count == 0)
        {
            return;
        }

        var tenantIds = members.Select(member => member.TenantId).Distinct().ToList();
        var existingNodes = await _db.ResourceNodes
            .AsNoTracking()
            .Where(node => tenantIds.Contains(node.TenantId) && node.ExternalKey.StartsWith("member:"))
            .Select(node => new { node.TenantId, node.ExternalKey })
            .ToListAsync();

        var existingKeys = existingNodes
            .Select(node => (node.TenantId, node.ExternalKey))
            .ToHashSet();

        var nodesToAdd = new List<ResourceNode>();
        foreach (var member in members)
        {
            string externalKey = $"member:{member.Id:D}";
            if (existingKeys.Contains((member.TenantId, externalKey)))
            {
                continue;
            }

            rootNodeLookup.TryGetValue(member.TenantId, out Guid? parentId);
            nodesToAdd.Add(ResourceNode.Create(member.DisplayName, externalKey, member.TenantId, parentId));
        }

        if (nodesToAdd.Count == 0)
        {
            return;
        }

        _db.ResourceNodes.AddRange(nodesToAdd);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} member resource nodes.", nodesToAdd.Count);
    }
}
