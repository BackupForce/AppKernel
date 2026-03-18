using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin.Winnings;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Winnings.Endpoints;

public static class GetWinningEndpoint
{
    public static RouteHandlerBuilder MapGetWinningEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{winningId:guid}",
                async (Guid winningId,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    GetAdminWinningByIdQuery query = new GetAdminWinningByIdQuery(tenantContext.TenantId, winningId);
                    return await UseCaseInvoker.Send<GetAdminWinningByIdQuery, AdminWinningDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingWinnings.Read.Name)
            .Produces<AdminWinningDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetWinningById");
    }
}
