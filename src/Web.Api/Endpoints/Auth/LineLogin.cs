using Application.Auth;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

public class LineLogin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{tenantId:guid}/auth/line-login", async (
            Guid tenantId,
            LineLoginRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (tenantId == Guid.Empty)
            {
                return Results.BadRequest();
            }

            LineLoginCommand command = new LineLoginCommand(request.AccessToken);
            Result<LineLoginResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
