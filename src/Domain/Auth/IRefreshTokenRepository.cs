namespace Domain.Auth;

public interface IRefreshTokenRepository
{
    Task<RefreshTokenRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshTokenRecord>> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    void Insert(RefreshTokenRecord record);
}
