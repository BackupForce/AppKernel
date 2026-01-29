namespace Application.Abstractions.Identity;

public interface ILineAuthService
{
    /// <summary>
    /// 中文註解：Application 只知道「拿 token 換 user id」，不關心 HTTP/JSON 細節。
    /// </summary>
    Task<ExternalIdentityResult> VerifyAccessTokenAsync(string accessToken, CancellationToken ct);
}

public sealed record ExternalIdentityResult(
    bool IsValid,
    string? LineUserId,
    string? ErrorCode,
    string? ErrorMessage,
    string? DisplayName = null,
    Uri? PictureUrl = null);
