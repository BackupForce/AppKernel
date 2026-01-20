namespace Application.Auth;

public sealed class RefreshTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public string? RefreshToken { get; init; }

    public Guid SessionId { get; init; }
}
