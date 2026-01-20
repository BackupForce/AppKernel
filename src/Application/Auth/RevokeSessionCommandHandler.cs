using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auth;
using SharedKernel;

namespace Application.Auth;

internal sealed class RevokeSessionCommandHandler(
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeSessionCommand>
{
    public async Task<Result> Handle(RevokeSessionCommand command, CancellationToken cancellationToken)
    {
        AuthSession? session = await authSessionRepository.GetByIdAsync(command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(AuthErrors.SessionNotFound(command.SessionId));
        }

        Guid? tenantId = userContext.TenantId;
        if (!tenantId.HasValue || session.TenantId != tenantId.Value || session.UserId != userContext.UserId)
        {
            return Result.Failure(AuthErrors.SessionRevoked);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;
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
