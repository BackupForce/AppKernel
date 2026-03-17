using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.Create;
using Application.Gaming.TicketTemplates.GetList;
using Application.Gaming.TicketTemplates.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Deprecated.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Deprecated.TicketTemplates.Endpoints;

public static class UpdateTicketTemplateEndpoint
{
    public static RouteHandlerBuilder MapUpdateTicketTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{templateId:guid}",
                async (Guid templateId, UpdateTicketTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateTicketTemplateCommand(
                        templateId,
                        request.Code,
                        request.Name,
                        request.Type,
                        request.Price,
                        request.ValidFrom,
                        request.ValidTo,
                        request.MaxLinesPerTicket);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateTicketTemplate");
    }
}
