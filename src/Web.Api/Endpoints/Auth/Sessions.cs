using Application.Auth;
using Asp.Versioning;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

public sealed class Sessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/sessions", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            GetSessionsQuery query = new();
            return await UseCaseInvoker.Send<GetSessionsQuery, IReadOnlyCollection<AuthSessionDto>>(
                query,
                sender,
                Results.Ok,
                cancellationToken);
        })
        .RequireAuthorization()
        .WithGroupName("auth-v1")
        .WithMetadata(new ApiVersion(1, 0))
        .WithTags(Tags.Auth);

        app.MapDelete("auth/sessions/{sessionId:guid}", async (
            Guid sessionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RevokeSessionCommand command = new(sessionId);
            Result result = await sender.Send(command, cancellationToken);

            return result.Match(
                onSuccess: () => Results.Ok(),
                onFailure: CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithGroupName("auth-v1")
        .WithMetadata(new ApiVersion(1, 0))
        .WithTags(Tags.Auth);
    }
}
