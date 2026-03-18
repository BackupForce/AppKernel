using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.End;
using Application.Gaming.TicketClaimEvents.Update;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class EndTicketClaimEventEndpoint
{
    public static RouteHandlerBuilder MapEndTicketClaimEventEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{id:guid}/end",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    EndTicketClaimEventCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send<EndTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.GamingTicketClaimEvent.End.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminEndTicketClaimEvent");
    }
}
