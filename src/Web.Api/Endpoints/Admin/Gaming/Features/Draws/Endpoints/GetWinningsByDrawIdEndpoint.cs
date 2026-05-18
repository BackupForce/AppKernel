using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.ManualClose;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin.Winnings;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetWinningsByDrawIdEndpoint
{
    public static RouteHandlerBuilder MapGetWinningsByDrawIdEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawId:guid}/winnings",
                async (Guid drawId,
                    [AsParameters] GetAdminWinningsByDrawRequest request,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    if (request.Current < 1)
                    {
                        return Results.BadRequest("Current must be greater than or equal to 1.");
                    }

                    if (request.PageSize < 1 || request.PageSize > 200)
                    {
                        return Results.BadRequest("PageSize must be between 1 and 200.");
                    }

                    GetAdminWinningsByDrawIdQuery query = new GetAdminWinningsByDrawIdQuery(
                        tenantContext.TenantId,
                        drawId,
                        request.Q,
                        request.Redeemed,
                        request.Current,
                        request.PageSize);

                    return await UseCaseInvoker.Send<GetAdminWinningsByDrawIdQuery, PagedResult<AdminWinningListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingWinnings.Read.Name)
            .Produces<PagedResult<AdminWinningListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetWinningsByDrawId");
    }
}
