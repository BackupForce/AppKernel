using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Auth;
using Domain.Auth;
using Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SharedKernel;
using System.Reflection;

namespace Application.UnitTests.Authentication;

public class RefreshTokenCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenGenerator _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
    private readonly IRefreshTokenHasher _refreshTokenHasher = Substitute.For<IRefreshTokenHasher>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AuthTokenOptions _options = new()
    {
        AccessTokenTtlMinutes = 15,
        RefreshTokenTtlDays = 30,
        RefreshTokenPepper = "pepper"
    };

    [Fact]
    public async Task Handle_ShouldRotateRefreshToken_WhenValid()
    {
        DateTime utcNow = new(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(utcNow);

        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        AuthSession session = AuthSession.Create(tenantId, userId, utcNow, utcNow.AddDays(30), null, null, null);
        RefreshTokenRecord record = RefreshTokenRecord.Create(session.Id, "hash", utcNow, utcNow.AddDays(30));
        AttachSession(record, session);

        _refreshTokenHasher.Hash("rt").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(record);

        User user = CreateUser(userId, tenantId);
        _userRepository.GetByIdWithRolesAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        _refreshTokenGenerator.GenerateToken().Returns("new-rt");
        _refreshTokenHasher.Hash("new-rt").Returns("new-hash");
        _jwtService.IssueAccessToken(
            user.Id,
            user.Name.ToString(),
            user.Type,
            user.TenantId,
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<IEnumerable<string>>(),
            utcNow)
            .Returns(("access", utcNow.AddMinutes(15)));

        RefreshTokenRecord? insertedRecord = null;
        _refreshTokenRepository.Insert(Arg.Do<RefreshTokenRecord>(record => insertedRecord = record));

        RefreshTokenCommandHandler handler = CreateHandler();
        Result<RefreshTokenResponse> result = await handler.Handle(new RefreshTokenCommand("rt", null, null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshToken.Should().Be("new-rt");
        record.RevokedReason.Should().Be("rotated");
        record.ReplacedByTokenId.Should().Be(insertedRecord?.Id);
        session.LastUsedAtUtc.Should().Be(utcNow);
    }

    [Fact]
    public async Task Handle_ShouldRevokeSession_WhenReuseDetected()
    {
        DateTime utcNow = new(2024, 01, 02, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(utcNow);

        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        AuthSession session = AuthSession.Create(tenantId, userId, utcNow.AddDays(-1), utcNow.AddDays(30), null, null, null);
        RefreshTokenRecord record = RefreshTokenRecord.Create(session.Id, "hash", utcNow.AddDays(-1), utcNow.AddDays(30));
        record.MarkRotated(Guid.NewGuid(), utcNow.AddMinutes(-10));
        AttachSession(record, session);

        _refreshTokenHasher.Hash("rt").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(record);
        _refreshTokenRepository.GetBySessionIdAsync(session.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshTokenRecord> { record });

        RefreshTokenCommandHandler handler = CreateHandler();
        Result<RefreshTokenResponse> result = await handler.Handle(new RefreshTokenCommand("rt", null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthErrors.RefreshTokenReused);
        session.RevokedAtUtc.Should().Be(utcNow);
        session.RevokeReason.Should().Be("refresh_token_reused");
    }

    [Fact]
    public async Task Handle_ShouldReturnExpired_WhenTokenExpired()
    {
        DateTime utcNow = new(2024, 01, 03, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(utcNow);

        AuthSession session = AuthSession.Create(Guid.NewGuid(), Guid.NewGuid(), utcNow.AddDays(-2), utcNow.AddDays(30), null, null, null);
        RefreshTokenRecord record = RefreshTokenRecord.Create(session.Id, "hash", utcNow.AddDays(-2), utcNow.AddDays(-1));
        AttachSession(record, session);

        _refreshTokenHasher.Hash("rt").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(record);

        RefreshTokenCommandHandler handler = CreateHandler();
        Result<RefreshTokenResponse> result = await handler.Handle(new RefreshTokenCommand("rt", null, null), default);

        result.Error.Should().Be(AuthErrors.RefreshTokenExpired);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalid_WhenTokenMissing()
    {
        _refreshTokenHasher.Hash("rt").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns((RefreshTokenRecord?)null);

        RefreshTokenCommandHandler handler = CreateHandler();
        Result<RefreshTokenResponse> result = await handler.Handle(new RefreshTokenCommand("rt", null, null), default);

        result.Error.Should().Be(AuthErrors.InvalidRefreshToken);
    }

    private RefreshTokenCommandHandler CreateHandler()
    {
        IAuthTokenSettings authTokenSettings = Substitute.For<IAuthTokenSettings>();
        authTokenSettings.RefreshTokenTtlDays.Returns(_options.RefreshTokenTtlDays);

        return new RefreshTokenCommandHandler(
            _refreshTokenRepository,
            _userRepository,
            _refreshTokenGenerator,
            _refreshTokenHasher,
            _jwtService,
            _dateTimeProvider,
            _unitOfWork,
            authTokenSettings);
    }

    private static User CreateUser(Guid userId, Guid tenantId)
    {
        Result<Email> email = Email.Create("test@example.com");
        User user = User.Create(
            email.Value,
            new Name("Tester"),
            "hash",
            false,
            UserType.Tenant,
            tenantId);

        typeof(User).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(user, userId);

        return user;
    }

    private static void AttachSession(RefreshTokenRecord record, AuthSession session)
    {
        typeof(RefreshTokenRecord)
            .GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
            .SetValue(record, session);
    }
}
