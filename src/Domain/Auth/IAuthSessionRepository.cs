namespace Domain.Auth;

public interface IAuthSessionRepository
{
    Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSession>> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    void Insert(AuthSession session);
}
