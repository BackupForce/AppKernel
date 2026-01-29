using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Members.Profiles;
using Domain.Members;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Members.Profiles;

public class UpsertMemberProfileCommandHandlerTests
{
    private readonly IMemberRepository _memberRepository = Substitute.For<IMemberRepository>();
    private readonly IMemberProfileRepository _memberProfileRepository = Substitute.For<IMemberProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Fact]
    public async Task Handle_Should_CreateProfile_WhenMissing()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(tenantId);

        Member member = Member.Create(tenantId, Guid.NewGuid(), "M001", "Test", DateTime.UtcNow).Value;
        _memberRepository.GetByIdAsync(tenantId, memberId, Arg.Any<CancellationToken>())
            .Returns(member);

        _memberProfileRepository.GetByMemberIdAsync(memberId, Arg.Any<CancellationToken>())
            .Returns((MemberProfile?)null);

        DateTime now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);

        UpsertMemberProfileCommandHandler handler = new(
            _memberRepository,
            _memberProfileRepository,
            _unitOfWork,
            _tenantContext,
            _dateTimeProvider);

        Result<MemberProfileDto> result = await handler.Handle(
            new UpsertMemberProfileCommand(memberId, "  Real Name ", Gender.Male, " 0912 ", true),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value.RealName.Should().Be("Real Name");
        result.Value.PhoneNumber.Should().Be("0912");
        result.Value.Gender.Should().Be(Gender.Male);
        result.Value.UpdatedAtUtc.Should().Be(now);

        _memberProfileRepository.Received(1).Insert(Arg.Is<MemberProfile>(profile =>
            profile.MemberId == memberId &&
            profile.RealName == "Real Name" &&
            profile.PhoneNumber == "0912" &&
            profile.Gender == Gender.Male &&
            profile.PhoneVerified));
        _memberProfileRepository.DidNotReceive().Update(Arg.Any<MemberProfile>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UpdateProfile_WhenExists()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(tenantId);

        Member member = Member.Create(tenantId, Guid.NewGuid(), "M002", "Test", DateTime.UtcNow).Value;
        _memberRepository.GetByIdAsync(tenantId, memberId, Arg.Any<CancellationToken>())
            .Returns(member);

        MemberProfile profile = MemberProfile.Create(memberId, "Old", Gender.Unknown, "123", false, DateTime.UtcNow.AddDays(-1));
        _memberProfileRepository.GetByMemberIdAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(profile);

        DateTime now = DateTime.UtcNow;
        _dateTimeProvider.UtcNow.Returns(now);

        UpsertMemberProfileCommandHandler handler = new(
            _memberRepository,
            _memberProfileRepository,
            _unitOfWork,
            _tenantContext,
            _dateTimeProvider);

        Result<MemberProfileDto> result = await handler.Handle(
            new UpsertMemberProfileCommand(memberId, "New Name", Gender.Female, null, false),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value.RealName.Should().Be("New Name");
        result.Value.Gender.Should().Be(Gender.Female);
        result.Value.PhoneNumber.Should().BeNull();
        result.Value.UpdatedAtUtc.Should().Be(now);

        _memberProfileRepository.DidNotReceive().Insert(Arg.Any<MemberProfile>());
        _memberProfileRepository.Received(1).Update(profile);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenMemberMissing()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(tenantId);

        _memberRepository.GetByIdAsync(tenantId, memberId, Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        UpsertMemberProfileCommandHandler handler = new(
            _memberRepository,
            _memberProfileRepository,
            _unitOfWork,
            _tenantContext,
            _dateTimeProvider);

        Result<MemberProfileDto> result = await handler.Handle(
            new UpsertMemberProfileCommand(memberId, "Name", Gender.Other, null, false),
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MemberErrors.MemberNotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
