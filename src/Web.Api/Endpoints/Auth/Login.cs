using Application.Abstractions.Authorization;
using Application.Auth;
using Asp.Versioning;
using MediatR;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;
public class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            LoginRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            LoginCommand command = new LoginCommand(request.Email, request.Password, request.TenantCode);

            Result<LoginResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .AllowAnonymous()
		.WithGroupName("auth-v1")
		.WithMetadata(new ApiVersion(1, 0))
        .WithTags(Tags.Auth);
	}
}
