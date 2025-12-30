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
        app.MapPost("auth/line/login", async (
            LineLoginRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginOrRegisterByLineCommand(request.LineUserId, request.LineUserName);

            Result<LineLoginResultDto> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
