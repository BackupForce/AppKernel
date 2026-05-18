using Application.Abstractions.Authentication;
using Application.Auth;
using Asp.Versioning;
using MediatR;
using Microsoft.Extensions.Options;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;
public class LineLogin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/line-login", async (
            LineLoginRequest request,
            HttpContext httpContext,
            ISender sender,
            IOptions<AuthTokenOptions> authTokenOptions,
            CancellationToken cancellationToken) =>
        {
            string? userAgent = httpContext.Request.Headers.UserAgent.ToString();
            string? ip = httpContext.Connection.RemoteIpAddress?.ToString();

            LineLoginCommand command = new LineLoginCommand(request.AccessToken, request.DeviceId, userAgent, ip);

            Result<LineLoginResponse> result = await sender.Send(command, cancellationToken);

            AuthTokenOptions options = authTokenOptions.Value;
            return result.Match(response =>
            {
                LineLoginResponse payload = response;
                if (options.UseRefreshTokenCookie && response.RefreshToken is not null)
                {
                    DateTime refreshExpiresAtUtc = DateTime.UtcNow.AddDays(options.RefreshTokenTtlDays);
                    RefreshTokenCookieHelper.AppendRefreshTokenCookie(
                        httpContext.Response,
                        options,
                        response.RefreshToken,
                        refreshExpiresAtUtc);

                    payload = new LineLoginResponse
                    {
                        AccessToken = response.AccessToken,
                        AccessTokenExpiresAtUtc = response.AccessTokenExpiresAtUtc,
                        RefreshToken = null,
                        SessionId = response.SessionId,
                        UserId = response.UserId,
                        TenantId = response.TenantId,
                        MemberId = response.MemberId,
                        MemberNo = response.MemberNo
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
