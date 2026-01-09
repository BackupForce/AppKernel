using System.Security.Claims;
using Application.Abstractions.Authentication;
using Domain.Users;
using FluentAssertions;

namespace Application.UnitTests.Authentication;

public class JwtUserContextTests
{
    [Fact]
    public void TryFromClaims_Should_ReturnFalse_When_UserType_Missing()
    {
        ClaimsPrincipal principal = BuildPrincipal(
            Guid.NewGuid(),
            userType: null,
            tenantId: null);

        bool result = JwtUserContext.TryFromClaims(principal, out JwtUserContext? context);

        result.Should().BeFalse();
        context.Should().BeNull();
    }

    [Fact]
    public void TryFromClaims_Should_ReturnTrue_For_Platform_Without_Tenant()
    {
        ClaimsPrincipal principal = BuildPrincipal(
            Guid.NewGuid(),
            UserType.Platform.ToString(),
            null);

        bool result = JwtUserContext.TryFromClaims(principal, out JwtUserContext? context);

        result.Should().BeTrue();
        context!.UserType.Should().Be(UserType.Platform);
        context.TenantId.Should().BeNull();
    }

    [Fact]
    public void TryFromClaims_Should_ReturnFalse_For_Platform_With_Tenant()
    {
        ClaimsPrincipal principal = BuildPrincipal(
            Guid.NewGuid(),
            UserType.Platform.ToString(),
            Guid.NewGuid());

        bool result = JwtUserContext.TryFromClaims(principal, out JwtUserContext? context);

        result.Should().BeFalse();
        context.Should().BeNull();
    }

    [Fact]
    public void TryFromClaims_Should_ReturnFalse_For_Tenant_Missing_TenantId()
    {
        ClaimsPrincipal principal = BuildPrincipal(
            Guid.NewGuid(),
            UserType.Tenant.ToString(),
            null);

        bool result = JwtUserContext.TryFromClaims(principal, out JwtUserContext? context);

        result.Should().BeFalse();
        context.Should().BeNull();
    }

    [Fact]
    public void TryFromClaims_Should_ReturnTrue_For_Tenant_With_TenantId()
    {
        Guid tenantId = Guid.NewGuid();
        ClaimsPrincipal principal = BuildPrincipal(
            Guid.NewGuid(),
            UserType.Tenant.ToString(),
            tenantId);

        bool result = JwtUserContext.TryFromClaims(principal, out JwtUserContext? context);

        result.Should().BeTrue();
        context!.UserType.Should().Be(UserType.Tenant);
        context.TenantId.Should().Be(tenantId);
    }

    private static ClaimsPrincipal BuildPrincipal(Guid userId, string? userType, Guid? tenantId)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (!string.IsNullOrWhiteSpace(userType))
        {
            claims.Add(new Claim(JwtClaimNames.UserType, userType));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim(JwtClaimNames.TenantId, tenantId.Value.ToString("D")));
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}
