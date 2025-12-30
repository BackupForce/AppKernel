using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Members.Points.Adjust;
using Domain.Members;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Members;

public sealed class AdjustMemberPointsCommandTests
{
    private readonly AdjustMemberPointsCommandHandler _handler;
    private readonly IMemberRepository _memberRepository = Substitute.For<IMemberRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IDbTransaction _transaction = Substitute.For<IDbTransaction>();

    public AdjustMemberPointsCommandTests()
    {
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _userContext.UserId.Returns(Guid.NewGuid());
        _unitOfWork.BeginTransactionAsync().Returns(Task.FromResult(_transaction));

        _handler = new AdjustMemberPointsCommandHandler(
            _memberRepository,
            _unitOfWork,
            _dateTimeProvider,
            _userContext,
            _cacheService);
    }

    [Fact]
    public async Task Handle_Should_AddPoints_WhenBalanceIsValid()
    {
        // 安排
        Result<Member> memberResult = Member.Create(null, "MBR-001", "測試會員", DateTime.UtcNow);
        Member member = memberResult.Value;
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _memberRepository.GetPointBalanceAsync(member.Id, Arg.Any<CancellationToken>())
            .Returns(MemberPointBalance.Create(member.Id, DateTime.UtcNow));

        var command = new AdjustMemberPointsCommand(member.Id, 100, "手動加點");

        // 動作
        Result<long> result = await _handler.Handle(command, CancellationToken.None);

        // 斷言
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
        _memberRepository.Received().InsertPointLedger(Arg.Any<MemberPointLedger>());
        await _unitOfWork.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
        _transaction.Received().Commit();
    }

    [Fact]
    public async Task Handle_Should_Fail_WhenBalanceWouldBeNegative()
    {
        // 安排
        Result<Member> memberResult = Member.Create(null, "MBR-002", "測試會員", DateTime.UtcNow);
        Member member = memberResult.Value;
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        MemberPointBalance balance = MemberPointBalance.Create(member.Id, DateTime.UtcNow);
        _memberRepository.GetPointBalanceAsync(member.Id, Arg.Any<CancellationToken>()).Returns(balance);

        var command = new AdjustMemberPointsCommand(member.Id, -50, "手動扣點");

        // 動作
        Result<long> result = await _handler.Handle(command, CancellationToken.None);

        // 斷言
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MemberErrors.NegativePointBalance);
    }
}
