namespace Application.Auth;

public sealed class LogoutRequest
{
    public string? RefreshToken { get; init; }
}
