namespace Application.Auth;

public sealed class LineLiffLoginRequest
{
    public string AccessToken { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? PictureUrl { get; init; }
    public string? Email { get; init; }
    public string? DeviceId { get; init; }
}
