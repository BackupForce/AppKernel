using Application.Abstractions.Caching;
using Application.IntegrationTests.Infrastructure;
using Domain.Security;
using Domain.Tenants;
using Domain.Users;
using FluentAssertions;
using Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests.Authorization;

public class PermissionProviderTests : BaseIntegrationTest
{
    public PermissionProviderTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task HasPermissionAsync_Should_Deny_When_NodeTenantMismatch()
    {
        var tenantAId = Guid.NewGuid();
        var tenantA = Tenant.Create(tenantAId, "TNA", "Tenant A");
        var tenantBId = Guid.NewGuid();
        var tenantB = Tenant.Create(tenantBId, "TNB", "Tenant B");

        DbContext.Tenants.AddRange(tenantA, tenantB);

        Email email = Email.Create("user1@example.com").Value;
        var name = new Name("User One");
        var user = User.Create(email, name, "hash", true, UserType.Tenant, tenantBId);
        DbContext.Users.Add(user);

        var nodeA = ResourceNode.Create("Node A", "node-a", tenantAId);
        DbContext.ResourceNodes.Add(nodeA);

        var assignment = PermissionAssignment.Create(
            SubjectType.User,
            Decision.Allow,
            user.Id,
            "Resource.Read",
            tenantBId,
            nodeA.Id);
        DbContext.PermissionAssignments.Add(assignment);

        await DbContext.SaveChangesAsync();

        PermissionProvider provider = CreateProvider();

        bool result = await provider.HasPermissionAsync(user.Id, "Resource.Read", nodeA.Id, tenantBId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_Should_Allow_When_LineageInSameTenant()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create(tenantId, "TNC", "Tenant C");
        DbContext.Tenants.Add(tenant);

        Email email = Email.Create("user2@example.com").Value;
        var name = new Name("User Two");
        var user = User.Create(email, name, "hash", true, UserType.Tenant, tenantId);
        DbContext.Users.Add(user);

        var rootNode = ResourceNode.Create("Root", "root-lineage", tenantId);
        var childNode = ResourceNode.Create("Child", "child-lineage", tenantId, rootNode.Id);
        DbContext.ResourceNodes.AddRange(rootNode, childNode);

        var assignment = PermissionAssignment.Create(
            SubjectType.User,
            Decision.Allow,
            user.Id,
            "Resource.Read",
            tenantId,
            rootNode.Id);
        DbContext.PermissionAssignments.Add(assignment);

        await DbContext.SaveChangesAsync();

        PermissionProvider provider = CreateProvider();

        bool result = await provider.HasPermissionAsync(user.Id, "Resource.Read", childNode.Id, tenantId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_Should_Deny_When_LineageHasCycle()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create(tenantId, "TND", "Tenant D");
        DbContext.Tenants.Add(tenant);

        Email email = Email.Create("user3@example.com").Value;
        var name = new Name("User Three");
        var user = User.Create(email, name, "hash", true, UserType.Tenant, tenantId);
        DbContext.Users.Add(user);

        var nodeA = ResourceNode.Create("Node A", "node-a-cycle", tenantId);
        var nodeB = ResourceNode.Create("Node B", "node-b-cycle", tenantId);
        DbContext.ResourceNodes.AddRange(nodeA, nodeB);

        var assignment = PermissionAssignment.Create(
            SubjectType.User,
            Decision.Allow,
            user.Id,
            "Resource.Read",
            tenantId,
            nodeA.Id);
        DbContext.PermissionAssignments.Add(assignment);

        await DbContext.SaveChangesAsync();

        await DbContext.Database.ExecuteSqlRawAsync(
            "UPDATE public.resource_nodes SET parent_id = {0} WHERE id = {1};", nodeB.Id, nodeA.Id);
        await DbContext.Database.ExecuteSqlRawAsync(
            "UPDATE public.resource_nodes SET parent_id = {0} WHERE id = {1};", nodeA.Id, nodeB.Id);

        PermissionProvider provider = CreateProvider();

        bool result = await provider.HasPermissionAsync(user.Id, "Resource.Read", nodeA.Id, tenantId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_Should_Allow_WhenGameNodeCoversPlayNode()
    {
        var tenantId = Guid.NewGuid();
        var tenant = Tenant.Create(tenantId, "TNE", "Tenant E");
        DbContext.Tenants.Add(tenant);

        Email email = Email.Create("user4@example.com").Value;
        var name = new Name("User Four");
        var user = User.Create(email, name, "hash", true, UserType.Tenant, tenantId);
        DbContext.Users.Add(user);

        var rootNode = ResourceNode.Create("Root", "root-gaming", tenantId);
        var gameNode = ResourceNode.Create("LOTTERY539", "game:LOTTERY539", tenantId, rootNode.Id);
        var playNode = ResourceNode.Create("BASIC", "play:LOTTERY539:BASIC", tenantId, gameNode.Id);
        DbContext.ResourceNodes.AddRange(rootNode, gameNode, playNode);

        var assignment = PermissionAssignment.Create(
            SubjectType.User,
            Decision.Allow,
            user.Id,
            "GAMING:DRAW:CREATE",
            tenantId,
            gameNode.Id);
        DbContext.PermissionAssignments.Add(assignment);

        await DbContext.SaveChangesAsync();

        PermissionProvider provider = CreateProvider();

        bool result = await provider.HasPermissionAsync(user.Id, "GAMING:DRAW:CREATE", playNode.Id, tenantId);

        result.Should().BeTrue();
    }

    private PermissionProvider CreateProvider()
    {
        ICacheService cacheService = ServiceProvider.GetRequiredService<ICacheService>();
        return new PermissionProvider(DbContext, cacheService);
    }
}
