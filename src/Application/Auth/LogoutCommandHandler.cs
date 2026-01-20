using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auth;
using SharedKernel;

namespace Application.Auth;

internal sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenHasher refreshTokenHasher,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Failure(AuthErrors.InvalidRefreshToken);
        }

        string tokenHash = refreshTokenHasher.Hash(command.RefreshToken);
        RefreshTokenRecord? tokenRecord = await refreshTokenRepository.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (tokenRecord?.Session is null)
        {
            return Result.Failure(AuthErrors.InvalidRefreshToken);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
        AuthSession session = tokenRecord.Session;
        session.Revoke("logout", utcNow);

        IReadOnlyList<RefreshTokenRecord> tokens = await refreshTokenRepository.GetBySessionIdAsync(
            session.Id,
            cancellationToken);
        foreach (RefreshTokenRecord token in tokens)
        {
            token.Revoke("logout", utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
