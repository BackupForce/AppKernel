using Application.Users.Activation;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class EnableUserEndpoint
{
    public static RouteHandlerBuilder MapEnableUserEndpoint(this RouteGroupBuilder group)
    {
        return group.MapPost("/{id:guid}/enable", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                EnableUserCommand command = new EnableUserCommand(id);
                return await UseCaseInvoker.Send(command, sender, ct);
            })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("EnableUser");
    }
}
