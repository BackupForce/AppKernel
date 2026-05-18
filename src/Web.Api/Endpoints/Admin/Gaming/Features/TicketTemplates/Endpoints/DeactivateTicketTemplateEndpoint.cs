using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.Activate;
using Application.Gaming.TicketTemplates.Deactivate;
using Application.Gaming.TicketTemplates.GetList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class DeactivateTicketTemplateEndpoint
{
    public static RouteHandlerBuilder MapDeactivateTicketTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return
        group.MapPatch(
                "/{templateId:guid}/deactivate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    var command = new DeactivateTicketTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("DeactivateTicketTemplate");
    }
}
