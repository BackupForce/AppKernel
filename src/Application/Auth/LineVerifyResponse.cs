using System.Text.Json.Serialization;

namespace Application.Auth;

public sealed class LineVerifyResponse
{
    [JsonPropertyName("sub")]
    public string LineUserId { get; init; } = string.Empty;
}
