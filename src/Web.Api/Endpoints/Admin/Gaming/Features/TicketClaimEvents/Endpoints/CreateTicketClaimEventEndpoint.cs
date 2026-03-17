using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Create;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class CreateTicketClaimEventEndpoint
{
    public static RouteHandlerBuilder MapCreateTicketClaimEventEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (Guid tenantId, CreateTicketClaimEventRequest request, ISender sender, CancellationToken ct) =>
                {
                    if (!EnumParsers.TryParseScopeType(request.ScopeType, out TicketClaimEventScopeType scopeType))
                    {
                        return Results.BadRequest("ScopeType must be SingleDraw or SingleDrawGroup.");
                    }

                    CreateTicketClaimEventCommand command = new(
                        tenantId,
                        request.Name,
                        request.StartsAtUtc,
                        request.EndsAtUtc,
                        request.TotalQuota,
                        request.PerMemberQuota,
                        scopeType,
                        request.ScopeId,
                        request.TicketTemplateId,
                        request.AllowedTagIds);

                    return await UseCaseInvoker.Send<CreateTicketClaimEventCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventCreate.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminCreateTicketClaimEvent");
    }
}
