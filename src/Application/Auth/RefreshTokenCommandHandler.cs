using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auth;
using Domain.Users;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.Auth;

internal sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IJwtService jwtService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IOptions<AuthTokenOptions> authTokenOptions)
    : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        AuthTokenOptions options = authTokenOptions.Value;
        DateTime utcNow = dateTimeProvider.UtcNow;
        string tokenHash = refreshTokenHasher.Hash(command.RefreshToken);

        RefreshTokenRecord? tokenRecord = await refreshTokenRepository.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (tokenRecord?.Session is null)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        AuthSession session = tokenRecord.Session;

        if (session.RevokedAtUtc.HasValue || session.ExpiresAtUtc <= utcNow)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.SessionRevoked);
        }

        if (tokenRecord.ExpiresAtUtc <= utcNow)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.RefreshTokenExpired);
        }

        if (tokenRecord.RevokedAtUtc.HasValue || tokenRecord.ReplacedByTokenId.HasValue)
        {
            if (tokenRecord.RevokedReason == "rotated" || tokenRecord.ReplacedByTokenId.HasValue)
            {
                await RevokeSessionAsync(session, utcNow, "refresh_token_reused", cancellationToken);
                return Result.Failure<RefreshTokenResponse>(AuthErrors.RefreshTokenReused);
            }

            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        User? user = await userRepository.GetByIdWithRolesAsync(session.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.SessionRevoked);
        }

        string newRefreshToken = refreshTokenGenerator.GenerateToken();
        string newRefreshTokenHash = refreshTokenHasher.Hash(newRefreshToken);

        RefreshTokenRecord newRecord = RefreshTokenRecord.Create(
            session.Id,
            newRefreshTokenHash,
            utcNow,
            utcNow.AddDays(options.RefreshTokenTtlDays));
        refreshTokenRepository.Insert(newRecord);

        tokenRecord.MarkRotated(newRecord.Id, utcNow);
        session.Touch(utcNow);

        var accessToken = jwtService.IssueAccessToken(
            user.Id,
            user.Name.ToString(),
            user.Type,
            user.TenantId,
            user.Roles.Select(r => r.Name).ToArray(),
            Array.Empty<Guid>(),
            Array.Empty<string>(),
            utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = newRefreshToken,
            SessionId = session.Id
        });
    }

    private async Task RevokeSessionAsync(
        AuthSession session,
        DateTime utcNow,
        string reason,
        CancellationToken cancellationToken)
    {
        session.Revoke(reason, utcNow);

        IReadOnlyList<RefreshTokenRecord> tokens = await refreshTokenRepository.GetBySessionIdAsync(
            session.Id,
            cancellationToken);
        foreach (RefreshTokenRecord token in tokens)
        {
            token.Revoke(reason, utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
