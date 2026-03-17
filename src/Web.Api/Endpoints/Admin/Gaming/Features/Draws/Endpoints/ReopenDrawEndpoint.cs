using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.Reopen;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class ReopenDrawEndpoint
{
    public static RouteHandlerBuilder MapReopenDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawId:guid}/reopen",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ReopenDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawReopen.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ReopenGameDraw");
    }
}
