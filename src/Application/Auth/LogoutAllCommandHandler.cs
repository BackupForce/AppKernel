using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auth;
using SharedKernel;

namespace Application.Auth;

internal sealed class LogoutAllCommandHandler(
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LogoutAllCommand>
{
    public async Task<Result> Handle(LogoutAllCommand command, CancellationToken cancellationToken)
    {
        Guid? tenantId = userContext.TenantId;
        if (!tenantId.HasValue)
        {
            return Result.Failure(AuthErrors.TenantContextMissing);
        }

        IReadOnlyList<AuthSession> sessions = await authSessionRepository.GetByUserAsync(
            tenantId.Value,
            userContext.UserId,
            cancellationToken);

        DateTime utcNow = dateTimeProvider.UtcNow;

        foreach (AuthSession session in sessions)
        {
            session.Revoke("logout_all", utcNow);

            IReadOnlyList<RefreshTokenRecord> tokens = await refreshTokenRepository.GetBySessionIdAsync(
                session.Id,
                cancellationToken);
            foreach (RefreshTokenRecord token in tokens)
            {
                token.Revoke("logout_all", utcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
