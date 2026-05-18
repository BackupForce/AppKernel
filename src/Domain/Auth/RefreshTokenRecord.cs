using SharedKernel;

namespace Domain.Auth;

public sealed class RefreshTokenRecord : Entity
{
    private RefreshTokenRecord(
        Guid id,
        Guid sessionId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
        : base(id)
    {
        SessionId = sessionId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    private RefreshTokenRecord()
    {
    }

    public Guid SessionId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? RevokedReason { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public AuthSession? Session { get; set; }

    public static RefreshTokenRecord Create(
        Guid sessionId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        return new RefreshTokenRecord(
            Guid.NewGuid(),
            sessionId,
            tokenHash,
            createdAtUtc,
            expiresAtUtc);
    }

    public void Revoke(string reason, DateTime utcNow)
    {
        if (RevokedAtUtc.HasValue)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        RevokedReason = reason;
    }

    public void MarkRotated(Guid replacedByTokenId, DateTime utcNow)
    {
        if (ReplacedByTokenId.HasValue)
        {
            return;
        }

        ReplacedByTokenId = replacedByTokenId;
        Revoke("rotated", utcNow);
    }
}
