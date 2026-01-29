using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authentication;

public sealed class LineIdentityVerifier(
    IHttpClientFactory httpClientFactory,
    IOptions<LineIdentityOptions> options,
    ILogger<LineIdentityVerifier> logger) : ILineAuthService
{
    public async Task<ExternalIdentityResult> VerifyAccessTokenAsync(string accessToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new ExternalIdentityResult(false, null, "token_required", "Access token is required.");
        }

        LineIdentityOptions opt = options.Value;
        if (opt.VerifyAccessTokenEndpoint is null || opt.ProfileEndpoint is null)
        {
            logger.LogError("中文註解：LINE Identity options 未設定完整（VerifyAccessTokenEndpoint / ProfileEndpoint）。");
            return new ExternalIdentityResult(false, null, "config_missing", "LINE endpoints are not configured.");
        }

        using HttpClient httpClient = httpClientFactory.CreateClient();

        // 1) Verify access token validity (LINE Login v2.1): GET /oauth2/v2.1/verify?access_token=...
        Uri verifyUri = BuildVerifyAccessTokenUri(opt.VerifyAccessTokenEndpoint, accessToken);

        using HttpRequestMessage verifyRequest = new HttpRequestMessage(HttpMethod.Get, verifyUri);
        using HttpResponseMessage verifyResponse = await httpClient.SendAsync(verifyRequest, ct);

        string verifyBody = await verifyResponse.Content.ReadAsStringAsync(ct);
        if (!verifyResponse.IsSuccessStatusCode)
        {
            return new ExternalIdentityResult(false, null, ((int)verifyResponse.StatusCode).ToString(System.Globalization.CultureInfo.InvariantCulture), verifyBody);
        }

        // 2) Get user profile with access token: GET /v2/profile  (Authorization: Bearer)
        using HttpRequestMessage profileRequest = new HttpRequestMessage(HttpMethod.Get, opt.ProfileEndpoint);
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using HttpResponseMessage profileResponse = await httpClient.SendAsync(profileRequest, ct);
        string profileBody = await profileResponse.Content.ReadAsStringAsync(ct);

        if (!profileResponse.IsSuccessStatusCode)
        {
            return new ExternalIdentityResult(false, null, ((int)profileResponse.StatusCode).ToString(System.Globalization.CultureInfo.InvariantCulture), profileBody);
        }

        LineProfileResponse? profile = TryDeserialize<LineProfileResponse>(profileBody);
        if (profile is null || string.IsNullOrWhiteSpace(profile.UserId))
        {
            return new ExternalIdentityResult(false, null, "invalid_profile", "LINE profile response missing userId.");
        }

        // LINE User ID = userId（Profile API）=> 作為 Member 唯一鍵
        return new ExternalIdentityResult(true, profile.UserId, null, null, profile.DisplayName, profile.PictureUrl);
    }

    private static Uri BuildVerifyAccessTokenUri(Uri baseEndpoint, string accessToken)
    {
        // baseEndpoint: https://api.line.me/oauth2/v2.1/verify
        // final:        https://api.line.me/oauth2/v2.1/verify?access_token=...
        UriBuilder builder = new UriBuilder(baseEndpoint);

        string query = builder.Query;
        if (!string.IsNullOrEmpty(query) && query.StartsWith('?'))
        {
            query = query.Substring(1);
        }

        string encodedToken = Uri.EscapeDataString(accessToken);
        builder.Query = string.IsNullOrWhiteSpace(query)
            ? $"access_token={encodedToken}"
            : $"{query}&access_token={encodedToken}";

        return builder.Uri;
    }

    private static T? TryDeserialize<T>(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private sealed class LineProfileResponse
    {
        [JsonPropertyName("userId")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; init; }

        [JsonPropertyName("pictureUrl")]
        public string? PictureUrl { get; init; }

        [JsonPropertyName("statusMessage")]
        public string? StatusMessage { get; init; }
    }
}
