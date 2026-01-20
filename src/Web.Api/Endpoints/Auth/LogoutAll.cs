using Application.Auth;
using Asp.Versioning;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

public sealed class LogoutAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout-all", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            LogoutAllCommand command = new();
            Result result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithGroupName("auth-v1")
        .WithMetadata(new ApiVersion(1, 0))
        .WithTags(Tags.Auth);
    }
}
