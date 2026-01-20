using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Auth;
using Domain.Auth;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Reflection;

namespace Application.UnitTests.Authentication;

public class LogoutCommandHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IRefreshTokenHasher _refreshTokenHasher = Substitute.For<IRefreshTokenHasher>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ShouldRevokeSession_WhenLogout()
    {
        DateTime utcNow = new(2024, 01, 10, 0, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.UtcNow.Returns(utcNow);

        AuthSession session = AuthSession.Create(Guid.NewGuid(), Guid.NewGuid(), utcNow.AddDays(-1), utcNow.AddDays(30), null, null, null);
        RefreshTokenRecord record = RefreshTokenRecord.Create(session.Id, "hash", utcNow.AddDays(-1), utcNow.AddDays(30));
        AttachSession(record, session);

        _refreshTokenHasher.Hash("rt").Returns("hash");
        _refreshTokenRepository.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(record);
        _refreshTokenRepository.GetBySessionIdAsync(session.Id, Arg.Any<CancellationToken>())
            .Returns(new List<RefreshTokenRecord> { record });

        LogoutCommandHandler handler = new(
            _refreshTokenRepository,
            _refreshTokenHasher,
            _dateTimeProvider,
            _unitOfWork);

        Result result = await handler.Handle(new LogoutCommand("rt"), default);

        result.IsSuccess.Should().BeTrue();
        session.RevokedAtUtc.Should().Be(utcNow);
        session.RevokeReason.Should().Be("logout");
    }

    private static void AttachSession(RefreshTokenRecord record, AuthSession session)
    {
        typeof(RefreshTokenRecord)
            .GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!
            .SetValue(record, session);
    }
}
