using Application.Abstractions.Data;
using Application.Gaming.Draws.Create;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetDrawTicketsEndpoint
{
    public static RouteHandlerBuilder MapGetDrawTicketsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawId:guid}/tickets",
                async (Guid drawId,
                    [AsParameters] GetDrawTicketsRequest request,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    GetDrawTicketsQuery query = new GetDrawTicketsQuery(drawId, request.Page, request.PageSize);
                    return await UseCaseInvoker.Send<GetDrawTicketsQuery, PagedResult<DrawTicketBetDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Tickets.Read.Name)
            .Produces<PagedResult<DrawTicketBetDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetDrawTickets");
    }
}
