using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication;

public sealed class LineIdentityVerifier(
    IHttpClientFactory httpClientFactory,
    ILogger<LineIdentityVerifier> logger) : IExternalIdentityVerifier
{
    private const string LineVerifyUrl = "https://api.line.me/oauth2/v2.1/verify";

    public async Task<ExternalIdentityResult> VerifyLineAccessTokenAsync(
        string accessToken,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new ExternalIdentityResult(false, null, "token_required", "Access token is required.");
        }

        // 中文註解：外部系統驗證細節集中於 Infrastructure。
        HttpClient httpClient = httpClientFactory.CreateClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, LineVerifyUrl)
        {
            Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("token", accessToken)
            })
        };

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "中文註解：LINE verify 連線逾時。");
            return new ExternalIdentityResult(false, null, "timeout", "LINE verify timeout.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "中文註解：LINE verify 連線失敗。");
            return new ExternalIdentityResult(false, null, "request_failed", ex.Message);
        }

        string responseBody = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            LineVerifyErrorResponse? errorResponse = TryDeserialize<LineVerifyErrorResponse>(responseBody);
            string errorCode = errorResponse?.Error ?? ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
            string errorMessage = errorResponse?.ErrorDescription ?? responseBody;

            return new ExternalIdentityResult(false, null, errorCode, errorMessage);
        }

        LineVerifySuccessResponse? verifyResponse = TryDeserialize<LineVerifySuccessResponse>(responseBody);
        if (verifyResponse is null || string.IsNullOrWhiteSpace(verifyResponse.LineUserId))
        {
            logger.LogWarning("中文註解：LINE verify 回應缺少使用者識別碼。");
            return new ExternalIdentityResult(false, null, "invalid_response", "LINE verify response missing user id.");
        }

        return new ExternalIdentityResult(true, verifyResponse.LineUserId, null, null);
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

    private sealed class LineVerifySuccessResponse
    {
        [JsonPropertyName("sub")]
        public string LineUserId { get; init; } = string.Empty;
    }

    private sealed class LineVerifyErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; init; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; init; }
    }
}
