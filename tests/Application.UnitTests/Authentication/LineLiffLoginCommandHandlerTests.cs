using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Auth;
using Domain.Auth;
using Domain.Members;
using Domain.Users;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Authentication;

public class LineLiffLoginCommandHandlerTests
{
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IUserLoginBindingReader _loginBindingReader = Substitute.For<IUserLoginBindingReader>();
    private readonly ILineLoginPersistenceService _lineLoginPersistenceService = Substitute.For<ILineLoginPersistenceService>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly ILineAuthService _lineAuthService = Substitute.For<ILineAuthService>();
    private readonly IMemberRepository _memberRepository = Substitute.For<IMemberRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_Should_CreateMemberAndLog_WhenNewMember()
    {
        Guid tenantId = Guid.NewGuid();
        SetupTenantContext(tenantId);

        _lineAuthService.VerifyAccessTokenAsync("token", Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityResult(true, "line-user", null, null));

        _loginBindingReader.FindUserByLoginAsync(tenantId, LoginProvider.Line, "line-user", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        User user = User.Create(Email.Create("line-user@test.com").Value, new Name("Line"), "hash", false, UserType.Member, tenantId);
        Member member = Member.Create(tenantId, user.Id, "M001", "Line", DateTime.UtcNow).Value;
        AuthSession session = AuthSession.Create(tenantId, user.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), null, null, null);

        _lineLoginPersistenceService.PersistAsync(
                tenantId,
                "line-user",
                "Line",
                null,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new LineLoginPersistenceResult(user, member, session, "rt", DateTime.UtcNow, true));

        _jwtService.IssueAccessToken(
                user.Id,
                user.Name.ToString(),
                user.Type,
                tenantId,
                Arg.Any<string[]>(),
                Arg.Any<Guid[]>(),
                Arg.Any<string[]>(),
                Arg.Any<DateTime>())
            .Returns(("jwt", DateTime.UtcNow.AddHours(1)));

        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        LineLiffLoginCommandHandler handler = new(
            _tenantContext,
            _loginBindingReader,
            _lineLoginPersistenceService,
            _jwtService,
            _lineAuthService,
            _memberRepository,
            _dateTimeProvider,
            _unitOfWork,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<LineLiffLoginCommandHandler>>());

        Result<LineLoginResponse> result = await handler.Handle(
            new LineLiffLoginCommand("token", null, null, null, null, null, null),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value.MemberId.Should().Be(member.Id);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _memberRepository.Received(1).InsertActivity(Arg.Is<MemberActivityLog>(log => log.Action == "member.auto_register.liff"));
    }

    [Fact]
    public async Task Handle_Should_ReturnExistingMember_WhenBindingFound()
    {
        Guid tenantId = Guid.NewGuid();
        SetupTenantContext(tenantId);

        _lineAuthService.VerifyAccessTokenAsync("token", Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityResult(true, "line-user", null, null));

        User existingUser = User.Create(Email.Create("line-user@test.com").Value, new Name("Line"), "hash", false, UserType.Member, tenantId);
        _loginBindingReader.FindUserByLoginAsync(tenantId, LoginProvider.Line, "line-user", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        Member member = Member.Create(tenantId, existingUser.Id, "M001", "Line", DateTime.UtcNow).Value;
        AuthSession session = AuthSession.Create(tenantId, existingUser.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), null, null, null);

        _lineLoginPersistenceService.PersistAsync(
                tenantId,
                "line-user",
                "Line",
                null,
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(new LineLoginPersistenceResult(existingUser, member, session, "rt", DateTime.UtcNow));

        _jwtService.IssueAccessToken(
                existingUser.Id,
                existingUser.Name.ToString(),
                existingUser.Type,
                tenantId,
                Arg.Any<string[]>(),
                Arg.Any<Guid[]>(),
                Arg.Any<string[]>(),
                Arg.Any<DateTime>())
            .Returns(("jwt", DateTime.UtcNow.AddHours(1)));

        LineLiffLoginCommandHandler handler = new(
            _tenantContext,
            _loginBindingReader,
            _lineLoginPersistenceService,
            _jwtService,
            _lineAuthService,
            _memberRepository,
            _dateTimeProvider,
            _unitOfWork,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<LineLiffLoginCommandHandler>>());

        Result<LineLoginResponse> result = await handler.Handle(
            new LineLiffLoginCommand("token", null, null, null, null, null, null),
            default);

        result.IsSuccess.Should().BeTrue();
        _memberRepository.DidNotReceive().InsertActivity(Arg.Any<MemberActivityLog>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenVerifyFails()
    {
        Guid tenantId = Guid.NewGuid();
        SetupTenantContext(tenantId);

        _lineAuthService.VerifyAccessTokenAsync("token", Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityResult(false, null, "invalid", "invalid"));

        LineLiffLoginCommandHandler handler = new(
            _tenantContext,
            _loginBindingReader,
            _lineLoginPersistenceService,
            _jwtService,
            _lineAuthService,
            _memberRepository,
            _dateTimeProvider,
            _unitOfWork,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<LineLiffLoginCommandHandler>>());

        Result<LineLoginResponse> result = await handler.Handle(
            new LineLiffLoginCommand("token", null, null, null, null, null, null),
            default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.LineVerifyFailed);
    }

    private void SetupTenantContext(Guid tenantId)
    {
        _tenantContext.TryGetTenantId(out Arg.Any<Guid>()).Returns(callInfo =>
        {
            callInfo[0] = tenantId;
            return true;
        });
    }
}
