using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.Update;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class UpdateTicketClaimEventEndpoint
{
    public static RouteHandlerBuilder MapUpdateTicketClaimEventEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{id:guid}",
                async (Guid tenantId, Guid id, UpdateTicketClaimEventRequest request, ISender sender, CancellationToken ct) =>
                {
                    if (!EnumParsers.TryParseScopeType(request.ScopeType, out TicketClaimEventScopeType scopeType))
                    {
                        return Results.BadRequest("ScopeType must be SingleDraw or SingleDrawGroup.");
                    }

                    UpdateTicketClaimEventCommand command = new(
                        tenantId,
                        id,
                        request.Name,
                        request.StartsAtUtc,
                        request.EndsAtUtc,
                        request.TotalQuota,
                        request.PerMemberQuota,
                        scopeType,
                        request.ScopeId,
                        request.TicketTemplateId,
                        request.AllowedTagIds);

                    return await UseCaseInvoker.Send<UpdateTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventUpdate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUpdateTicketClaimEvent");
    }
}
