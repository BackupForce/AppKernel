using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Activate;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.Update;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class ActivateTicketClaimEventEndpoint
{
    public static RouteHandlerBuilder MapActivateTicketClaimEventEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{id:guid}/activate",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    ActivateTicketClaimEventCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send<ActivateTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.GamingTicketClaimEvent.Activate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminActivateTicketClaimEvent");
    }
}
