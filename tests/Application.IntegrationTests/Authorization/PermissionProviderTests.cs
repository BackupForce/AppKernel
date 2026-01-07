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
        Guid tenantAId = Guid.NewGuid();
        Tenant tenantA = Tenant.Create(tenantAId, "TNA", "Tenant A");
        Guid tenantBId = Guid.NewGuid();
        Tenant tenantB = Tenant.Create(tenantBId, "TNB", "Tenant B");

        DbContext.Tenants.AddRange(tenantA, tenantB);

        Email email = Email.Create("user1@example.com").Value;
        Name name = new Name("User One");
        User user = User.Create(email, name, "hash", true);
        user.AssignTenant(tenantBId);
        DbContext.Users.Add(user);

        ResourceNode nodeA = ResourceNode.Create("Node A", "node-a", tenantAId);
        DbContext.ResourceNodes.Add(nodeA);

        PermissionAssignment assignment = PermissionAssignment.Create(
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
        Guid tenantId = Guid.NewGuid();
        Tenant tenant = Tenant.Create(tenantId, "TNC", "Tenant C");
        DbContext.Tenants.Add(tenant);

        Email email = Email.Create("user2@example.com").Value;
        Name name = new Name("User Two");
        User user = User.Create(email, name, "hash", true);
        user.AssignTenant(tenantId);
        DbContext.Users.Add(user);

        ResourceNode rootNode = ResourceNode.Create("Root", "root-lineage", tenantId);
        ResourceNode childNode = ResourceNode.Create("Child", "child-lineage", tenantId, rootNode.Id);
        DbContext.ResourceNodes.AddRange(rootNode, childNode);

        PermissionAssignment assignment = PermissionAssignment.Create(
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
        Guid tenantId = Guid.NewGuid();
        Tenant tenant = Tenant.Create(tenantId, "TND", "Tenant D");
        DbContext.Tenants.Add(tenant);

        Email email = Email.Create("user3@example.com").Value;
        Name name = new Name("User Three");
        User user = User.Create(email, name, "hash", true);
        user.AssignTenant(tenantId);
        DbContext.Users.Add(user);

        ResourceNode nodeA = ResourceNode.Create("Node A", "node-a-cycle", tenantId);
        ResourceNode nodeB = ResourceNode.Create("Node B", "node-b-cycle", tenantId);
        DbContext.ResourceNodes.AddRange(nodeA, nodeB);

        PermissionAssignment assignment = PermissionAssignment.Create(
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

    private PermissionProvider CreateProvider()
    {
        ICacheService cacheService = ServiceProvider.GetRequiredService<ICacheService>();
        return new PermissionProvider(DbContext, cacheService);
    }
}
