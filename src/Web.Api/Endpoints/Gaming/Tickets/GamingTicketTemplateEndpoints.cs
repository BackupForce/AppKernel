using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.Activate;
using Application.Gaming.TicketTemplates.Create;
using Application.Gaming.TicketTemplates.Deactivate;
using Application.Gaming.TicketTemplates.GetList;
using Application.Gaming.TicketTemplates.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.Tickets;

internal static class GamingTicketTemplateEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {

        group.MapGet(
                "/ticket-templates",
                async ([FromQuery] bool activeOnly, ISender sender, CancellationToken ct) =>
                {
                    GetTicketTemplatesQuery query = new GetTicketTemplatesQuery(activeOnly);
                    return await UseCaseInvoker.Send<GetTicketTemplatesQuery, IReadOnlyCollection<TicketTemplateDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<TicketTemplateDto>>(StatusCodes.Status200OK)
            .WithName("GetTicketTemplates");

        group.MapPost(
                "/ticket-templates",
                async (CreateTicketTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateTicketTemplateCommand command = new CreateTicketTemplateCommand(
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

        group.MapPut(
                "/ticket-templates/{templateId:guid}",
                async (Guid templateId, UpdateTicketTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateTicketTemplateCommand command = new UpdateTicketTemplateCommand(
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

        group.MapPatch(
                "/ticket-templates/{templateId:guid}/activate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    ActivateTicketTemplateCommand command = new ActivateTicketTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivateTicketTemplate");

        group.MapPatch(
                "/ticket-templates/{templateId:guid}/deactivate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    DeactivateTicketTemplateCommand command = new DeactivateTicketTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("DeactivateTicketTemplate");
    }
}
