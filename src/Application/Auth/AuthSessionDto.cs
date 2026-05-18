namespace Application.Auth;

public sealed class AuthSessionDto
{
    public Guid Id { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? LastUsedAtUtc { get; init; }

    public DateTime ExpiresAtUtc { get; init; }

    public DateTime? RevokedAtUtc { get; init; }

    public string? RevokeReason { get; init; }

    public string? UserAgent { get; init; }

    public string? Ip { get; init; }

    public string? DeviceId { get; init; }
}
