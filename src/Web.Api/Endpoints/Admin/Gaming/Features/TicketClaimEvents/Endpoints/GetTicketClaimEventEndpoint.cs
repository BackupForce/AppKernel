using Application.Abstractions.Authentication;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.GetById;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class GetTicketClaimEventEndpoint
{
    public static RouteHandlerBuilder MapGetTicketClaimEventEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{id:guid}",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    GetTicketClaimEventQuery query = new(tenantId, id);
                    return await UseCaseInvoker.Send<GetTicketClaimEventQuery, TicketClaimEventDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingTicketClaimEvent.Read.Name)
            .Produces<TicketClaimEventDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetTicketClaimEvent");
    }
}
