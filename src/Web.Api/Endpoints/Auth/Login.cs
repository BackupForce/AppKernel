using Application.Abstractions.Authentication;
using Application.Auth;
using Asp.Versioning;
using MediatR;
using Microsoft.Extensions.Options;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;
public class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            LoginRequest request,
            HttpContext httpContext,
            ISender sender,
            IOptions<AuthTokenOptions> authTokenOptions,
            CancellationToken cancellationToken) =>
        {
            string? userAgent = httpContext.Request.Headers.UserAgent.ToString();
            string? ip = httpContext.Connection.RemoteIpAddress?.ToString();

            LoginCommand command = new LoginCommand(
                request.Email,
                request.Password,
                request.TenantCode,
                request.DeviceId,
                userAgent,
                ip);

            Result<LoginResponse> result = await sender.Send(command, cancellationToken);

            AuthTokenOptions options = authTokenOptions.Value;
            return result.Match(response =>
            {
                LoginResponse payload = response;
                if (options.UseRefreshTokenCookie && response.RefreshToken is not null)
                {
                    DateTime refreshExpiresAtUtc = DateTime.UtcNow.AddDays(options.RefreshTokenTtlDays);
                    RefreshTokenCookieHelper.AppendRefreshTokenCookie(
                        httpContext.Response,
                        options,
                        response.RefreshToken,
                        refreshExpiresAtUtc);

                    payload = new LoginResponse
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
