using Domain.Users;
using FluentAssertions;

namespace Domain.UnitTests.Users;

public class UserTypeParserTests
{
    [Fact]
    public void TryParse_Should_ReturnTrue_For_Known_Types()
    {
        bool platformResult = UserTypeParser.TryParse("Platform", out UserType platform);
        bool tenantResult = UserTypeParser.TryParse("tenant", out UserType tenant);
        bool memberResult = UserTypeParser.TryParse("MEMBER", out UserType member);

        platformResult.Should().BeTrue();
        tenantResult.Should().BeTrue();
        memberResult.Should().BeTrue();
        platform.Should().Be(UserType.Platform);
        tenant.Should().Be(UserType.Tenant);
        member.Should().Be(UserType.Member);
    }

    [Fact]
    public void TryParse_Should_ReturnFalse_For_Unknown_Type()
    {
        bool result = UserTypeParser.TryParse("Unknown", out UserType parsed);

        result.Should().BeFalse();
        parsed.Should().Be(UserType.Tenant);
    }
}
