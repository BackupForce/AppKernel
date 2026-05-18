using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetMemberTicketsEndpoint
{
    public static RouteHandlerBuilder MapGetMemberTicketsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{memberId:guid}/tickets",
                async (Guid memberId,
                    [AsParameters] GetMemberTicketsRequest request,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    if (request.Page < 1)
                    {
                        return Results.BadRequest("Page must be greater than or equal to 1.");
                    }

                    if (request.PageSize < 1 || request.PageSize > 200)
                    {
                        return Results.BadRequest("PageSize must be between 1 and 200.");
                    }

                    GetMemberTicketsQuery query = new(tenantContext.TenantId, memberId, request.Page, request.PageSize);
                    return await UseCaseInvoker.Send<GetMemberTicketsQuery, PagedResult<DrawTicketBetDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Tickets.Read.Name)
            .Produces<PagedResult<DrawTicketBetDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberTickets");
    }
}
