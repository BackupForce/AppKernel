namespace Application.Auth;

public sealed class LineLoginRequest
{
    public string AccessToken { get; init; } = string.Empty;
    public string? DeviceId { get; init; }
}
