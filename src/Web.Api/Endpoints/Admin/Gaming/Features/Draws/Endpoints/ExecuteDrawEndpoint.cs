using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class ExecuteDrawEndpoint
{
    public static RouteHandlerBuilder MapExecuteDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawId:guid}/execute",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ExecuteDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawExecute.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ExecuteGameDraw");
    }
}
