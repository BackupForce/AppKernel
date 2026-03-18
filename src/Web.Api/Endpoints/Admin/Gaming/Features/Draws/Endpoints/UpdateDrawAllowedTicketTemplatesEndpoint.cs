using Application.Gaming.Draws.AllowedTicketTemplates.Update;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class UpdateDrawAllowedTicketTemplatesEndpoint
{
    public static RouteHandlerBuilder MapUpdateDrawAllowedTicketTemplatesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{drawId:guid}/allowed-ticket-templates",
                async (Guid drawId, UpdateDrawAllowedTicketTemplatesRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateDrawAllowedTicketTemplatesCommand(
                        drawId,
                        request.TemplateIds);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.GamingDraw.UpdateAllowedTemplates.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateGameDrawAllowedTicketTemplates");
    }
}
