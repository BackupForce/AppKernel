using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.Activate;
using Application.Gaming.TicketTemplates.GetList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Deprecated.TicketTemplates.Endpoints;

public static class ActivateTicketTemplateEndpoint
{
    public static RouteHandlerBuilder MapActivateTicketTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPatch(
                "/{templateId:guid}/activate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ActivateTicketTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivateTicketTemplate");
    }
}
