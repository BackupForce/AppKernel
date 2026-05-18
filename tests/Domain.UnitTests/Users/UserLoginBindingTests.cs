using Domain.Users;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Users;

public class UserLoginBindingTests
{
    [Fact]
    public void BindLogin_ShouldBeIdempotent_WhenSameProviderAndKey()
    {
        Email email = Email.Create("member@test.com").Value;
        User user = User.Create(email, new Name("Line Member"), "hash", false, UserType.Member, Guid.NewGuid());
        DateTime now = DateTime.UtcNow;

        Result first = user.BindLogin(LoginProvider.Line, "line-user-1", now);
        Result second = user.BindLogin(LoginProvider.Line, " line-user-1 ", now.AddMinutes(1));

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        user.LoginBindings.Should().HaveCount(1);
    }

    [Fact]
    public void BindLogin_ShouldRejectDifferentKey_ForSameProvider()
    {
        Email email = Email.Create("member2@test.com").Value;
        User user = User.Create(email, new Name("Line Member"), "hash", false, UserType.Member, Guid.NewGuid());
        DateTime now = DateTime.UtcNow;

        user.BindLogin(LoginProvider.Line, "line-user-1", now);
        Result result = user.BindLogin(LoginProvider.Line, "line-user-2", now.AddMinutes(1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.LoginProviderAlreadyBound(LoginProvider.Line));
    }
}
