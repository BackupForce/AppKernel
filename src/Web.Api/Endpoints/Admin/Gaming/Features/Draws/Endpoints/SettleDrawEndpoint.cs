using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class SettleDrawEndpoint
{
    public static RouteHandlerBuilder MapSettleDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawId:guid}/settle",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new SettleDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SettleGameDraw");
    }
}
