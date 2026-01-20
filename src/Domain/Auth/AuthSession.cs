using SharedKernel;

namespace Domain.Auth;

public sealed class AuthSession : Entity
{
    private readonly List<RefreshTokenRecord> _refreshTokens = new();

    private AuthSession(
        Guid id,
        Guid tenantId,
        Guid userId,
        DateTime createdAtUtc,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ip,
        string? deviceId)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        UserAgent = userAgent;
        Ip = ip;
        DeviceId = deviceId;
    }

    private AuthSession()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? LastUsedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? RevokeReason { get; private set; }

    public string? UserAgent { get; private set; }

    public string? Ip { get; private set; }

    public string? DeviceId { get; private set; }

    public IReadOnlyCollection<RefreshTokenRecord> RefreshTokens => _refreshTokens.ToList();

    public static AuthSession Create(
        Guid tenantId,
        Guid userId,
        DateTime utcNow,
        DateTime expiresAtUtc,
        string? userAgent,
        string? ip,
        string? deviceId)
    {
        return new AuthSession(
            Guid.NewGuid(),
            tenantId,
            userId,
            utcNow,
            expiresAtUtc,
            userAgent,
            ip,
            deviceId);
    }

    public void Touch(DateTime utcNow)
    {
        LastUsedAtUtc = utcNow;
    }

    public void Revoke(string reason, DateTime utcNow)
    {
        if (RevokedAtUtc.HasValue)
        {
            return;
        }

        RevokedAtUtc = utcNow;
        RevokeReason = reason;
    }
}
