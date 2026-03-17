using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Activate;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.Disable;
using Application.Gaming.TicketClaimEvents.Update;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class DisableTicketClaimEventEndpoint
{
    public static RouteHandlerBuilder MapDisableTicketClaimEventEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{id:guid}/disable",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    DisableTicketClaimEventCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send<DisableTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventDisable.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminDisableTicketClaimEvent");
    }
}
