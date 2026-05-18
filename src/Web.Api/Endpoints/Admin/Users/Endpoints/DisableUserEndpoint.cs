using Application.Users.Activation;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class DisableUserEndpoint
{
    public static RouteHandlerBuilder MapDisableUserEndpoint(this RouteGroupBuilder group)
    {
        return group.MapPost("/{id:guid}/disable", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                DisableUserCommand command = new DisableUserCommand(id);
                return await UseCaseInvoker.Send(command, sender, ct);
            })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DisableUser");
    }
}
