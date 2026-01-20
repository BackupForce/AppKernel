using Application.Abstractions.Authentication;
using Application.Auth;
using Asp.Versioning;
using MediatR;
using Microsoft.Extensions.Options;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

public sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            RefreshTokenRequest? request,
            HttpContext httpContext,
            ISender sender,
            IOptions<AuthTokenOptions> authTokenOptions,
            CancellationToken cancellationToken) =>
        {
            AuthTokenOptions options = authTokenOptions.Value;

            if (options.UseRefreshTokenCookie && !OriginValidationHelper.IsSameOrigin(httpContext.Request))
            {
                return CustomResults.Problem(Result.Failure(AuthErrors.InvalidRefreshToken));
            }

            string? refreshToken = options.UseRefreshTokenCookie
                ? httpContext.Request.Cookies[options.RefreshCookieName]
                : null;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = request?.RefreshToken;
            }

            RefreshTokenCommand command = new RefreshTokenCommand(
                refreshToken ?? string.Empty,
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString());

            Result<RefreshTokenResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(response =>
            {
                RefreshTokenResponse payload = response;
                if (options.UseRefreshTokenCookie && response.RefreshToken is not null)
                {
                    DateTime refreshExpiresAtUtc = DateTime.UtcNow.AddDays(options.RefreshTokenTtlDays);
                    RefreshTokenCookieHelper.AppendRefreshTokenCookie(
                        httpContext.Response,
                        options,
                        response.RefreshToken,
                        refreshExpiresAtUtc);

                    payload = new RefreshTokenResponse
                    {
                        AccessToken = response.AccessToken,
                        AccessTokenExpiresAtUtc = response.AccessTokenExpiresAtUtc,
                        RefreshToken = null,
                        SessionId = response.SessionId
                    };
                }

                return Results.Ok(payload);
            }, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithGroupName("auth-v1")
        .WithMetadata(new ApiVersion(1, 0))
        .WithTags(Tags.Auth);
    }
}
