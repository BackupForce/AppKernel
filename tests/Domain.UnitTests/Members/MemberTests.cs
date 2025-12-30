using Domain.Members;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Members;

public class MemberTests
{
    [Fact]
    public void Create_Should_Fail_When_DisplayName_Empty()
    {
        // 安排
        Guid? userId = null;
        string memberNo = "M-001";

        // 動作
        Result<Member> result = Member.Create(userId, memberNo, string.Empty, DateTime.UtcNow);

        // 斷言
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MemberErrors.DisplayNameRequired);
    }

    [Fact]
    public void Status_Flow_Should_Suspend_And_Activate()
    {
        // 安排
        Result<Member> memberResult = Member.Create(null, "M-002", "Test Member", DateTime.UtcNow);
        Member member = memberResult.Value;

        // 動作
        Result suspendResult = member.Suspend(DateTime.UtcNow);
        Result activateResult = member.Activate(DateTime.UtcNow);

        // 斷言
        suspendResult.IsSuccess.Should().BeTrue();
        member.Status.Should().Be(MemberStatus.Suspended);
        activateResult.IsSuccess.Should().BeTrue();
        member.Status.Should().Be(MemberStatus.Active);
    }
}
