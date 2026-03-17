using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.Create;
using Application.Gaming.TicketTemplates.GetList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class CreateTicketTemplateEndpoint
{
    public static RouteHandlerBuilder MapCreateTicketTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (CreateTicketTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreateTicketTemplateCommand(
                        request.Code,
                        request.Name,
                        request.Type,
                        request.Price,
                        request.ValidFrom,
                        request.ValidTo,
                        request.MaxLinesPerTicket);
                    return await UseCaseInvoker.Send<CreateTicketTemplateCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateTicketTemplate");
    }
}
