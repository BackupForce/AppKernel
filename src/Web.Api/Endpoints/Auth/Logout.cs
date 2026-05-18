using Application.Abstractions.Authentication;
using Application.Auth;
using Asp.Versioning;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

public sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            LogoutRequest? request,
            HttpContext httpContext,
            ISender sender,
            IOptions<AuthTokenOptions> authTokenOptions,
            IConfiguration configuration,
            CancellationToken cancellationToken) =>
        {
            AuthTokenOptions options = authTokenOptions.Value;

            if (options.UseRefreshTokenCookie)
            {
                string[] allowed = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                if (!OriginValidationHelper.IsAllowedOrigin(httpContext.Request, allowed))
                {
                    return CustomResults.Problem(Result.Failure(AuthErrors.InvalidRefreshTokenSameSite));
                }
            }


            string? refreshToken = options.UseRefreshTokenCookie
                ? httpContext.Request.Cookies[options.RefreshCookieName]
                : null;

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = request?.RefreshToken;
            }

            LogoutCommand command = new LogoutCommand(refreshToken ?? string.Empty);
            Result result = await sender.Send(command, cancellationToken);

            return result.Match(
                 onSuccess: () =>
                 {
                     if (options.UseRefreshTokenCookie)
                     {
                         RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, options);
                     }

                     return Results.Ok();
                 },
                 onFailure: CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithGroupName("auth-v1")
        .WithMetadata(new ApiVersion(1, 0))
        .WithTags(Tags.Auth);
    }
}
