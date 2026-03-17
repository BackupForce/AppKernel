using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.ManualClose;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class CloseDrawManuallyEndpoint
{
    public static RouteHandlerBuilder MapCloseDrawManuallyEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawId:guid}/manual-close",
                async (Guid drawId, CloseDrawManuallyRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CloseDrawManuallyCommand(drawId, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawManualClose.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CloseGameDrawManually");
    }
}
