using Application.Abstractions.Authorization;
using Domain.Security;
using FluentAssertions;
using Infrastructure.Authorization;
using NSubstitute;

namespace Infrastructure.UnitTests.Authorization;

public class PermissionEvaluatorTests
{
    [Fact]
    public async Task AuthorizeAsync_Should_ReturnTrue_WhenTenantWildcardMatches()
    {
        Guid callerUserId = Guid.NewGuid();
        Guid tenantId = Guid.NewGuid();
        IGrantedPermissionProvider provider = Substitute.For<IGrantedPermissionProvider>();
        IReadOnlySet<string> permissions = new HashSet<string> { "users:*" };
        provider.GetTenantPermissionsAsync(callerUserId, tenantId, Arg.Any<CancellationToken>())
            .Returns(permissions);

        PermissionEvaluator evaluator = new PermissionEvaluator(provider);
        PermissionRequirement requirement = new PermissionRequirement(
            "USERS:CREATE",
            PermissionScope.Tenant,
            tenantId,
            null);
        CallerContext callerContext = new CallerContext(callerUserId);

        bool result = await evaluator.AuthorizeAsync(requirement, callerContext, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizeAsync_Should_ReturnFalse_WhenTenantIdMissing()
    {
        Guid callerUserId = Guid.NewGuid();
        IGrantedPermissionProvider provider = Substitute.For<IGrantedPermissionProvider>();

        PermissionEvaluator evaluator = new PermissionEvaluator(provider);
        PermissionRequirement requirement = new PermissionRequirement(
            "USERS:CREATE",
            PermissionScope.Tenant,
            null,
            null);
        CallerContext callerContext = new CallerContext(callerUserId);

        bool result = await evaluator.AuthorizeAsync(requirement, callerContext, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeAsync_Should_ReturnFalse_WhenTargetUserIsDifferent()
    {
        Guid callerUserId = Guid.NewGuid();
        Guid targetUserId = Guid.NewGuid();
        IGrantedPermissionProvider provider = Substitute.For<IGrantedPermissionProvider>();

        PermissionEvaluator evaluator = new PermissionEvaluator(provider);
        PermissionRequirement requirement = new PermissionRequirement(
            "POINTS:ME:VIEW",
            PermissionScope.Self,
            null,
            targetUserId);
        CallerContext callerContext = new CallerContext(callerUserId);

        bool result = await evaluator.AuthorizeAsync(requirement, callerContext, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthorizeAsync_Should_ReturnFalse_WhenPlatformPermissionMissing()
    {
        Guid callerUserId = Guid.NewGuid();
        IGrantedPermissionProvider provider = Substitute.For<IGrantedPermissionProvider>();
        IReadOnlySet<string> permissions = new HashSet<string>();
        provider.GetPlatformPermissionsAsync(callerUserId, Arg.Any<CancellationToken>())
            .Returns(permissions);

        PermissionEvaluator evaluator = new PermissionEvaluator(provider);
        PermissionRequirement requirement = new PermissionRequirement(
            "TENANTS:CREATE",
            PermissionScope.Platform,
            null,
            null);
        CallerContext callerContext = new CallerContext(callerUserId);

        bool result = await evaluator.AuthorizeAsync(requirement, callerContext, CancellationToken.None);

        result.Should().BeFalse();
    }
}
