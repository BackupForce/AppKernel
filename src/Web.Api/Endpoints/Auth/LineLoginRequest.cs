namespace Web.Api.Endpoints.Auth;

public sealed class LineLoginRequest
{
    public string LineUserId { get; init; } = string.Empty;

    public string LineUserName { get; init; } = string.Empty;
}
