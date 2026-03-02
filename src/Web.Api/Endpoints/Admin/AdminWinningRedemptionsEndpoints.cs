using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin.Winnings;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminWinningRedemptionsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin/gaming")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Winnings");

        group.MapGet(
                "/draws/{drawId:guid}/winnings",
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
            .RequireAuthorization(Permission.Gaming.WinningsRead.Name)
            .Produces<PagedResult<AdminWinningListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetWinningsByDrawId");

        group.MapGet(
                "/winnings/{winningId:guid}",
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
            .RequireAuthorization(Permission.Gaming.WinningsRead.Name)
            .Produces<AdminWinningDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetWinningById");

        group.MapPost(
                "/winnings/{winningId:guid}/redeem",
                async (Guid winningId, ISender sender, CancellationToken ct) =>
                {
                    RedeemWinningCommand command = new RedeemWinningCommand(winningId);
                    return await UseCaseInvoker.Send<RedeemWinningCommand, AdminWinningDetailDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.WinningsRedeem.Name)
            .Produces<AdminWinningDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("AdminRedeemWinning");
    }
}
