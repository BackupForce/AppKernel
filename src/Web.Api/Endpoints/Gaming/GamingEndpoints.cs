using Application.Abstractions.Authorization;
using Application.Abstractions.Gaming;
using Application.Gaming.Awards.GetMy;
using Application.Gaming.Awards.Redeem;
using Application.Gaming.Catalog;
using Application.Gaming.Draws.AllowedTicketTemplates.Get;
using Application.Gaming.Draws.AllowedTicketTemplates.Update;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.ManualClose;
using Application.Gaming.Draws.Reopen;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Application.Gaming.Entitlements;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.Deactivate;
using Application.Gaming.Prizes.GetList;
using Application.Gaming.Prizes.Update;
using Application.Gaming.Tickets.GetMy;
using Application.Gaming.Tickets.Place;
using Application.Gaming.TicketTemplates.Activate;
using Application.Gaming.TicketTemplates.Create;
using Application.Gaming.TicketTemplates.Deactivate;
using Application.Gaming.TicketTemplates.GetList;
using Application.Gaming.TicketTemplates.Update;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming;

/// <summary>
/// Gaming 模組 API 路由，負責授權與 request/response 轉換。
/// </summary>
/// <remarks>
/// Web.Api 僅負責路由與授權，不承載業務邏輯。
/// </remarks>
public sealed class GamingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // 依租戶隔離路由，權限由 AuthorizationPolicy 控制。
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/gaming")
            .WithGroupName("tenant-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Gaming");

        Draws.GamingDrawEndpoints.Map(group);
        Campaigns.GamingCampaignEndpoints.Map(group);
        Tickets.GamingTicketTemplateEndpoints.Map(group);
        Prizes.GamingPrizeEndpoints.Map(group);
        Entitlements.GamingEntitlementEndpoints.Map(group);
        Catalog.GamingCatalogEndpoints.Map(group);
        Members.GamingMemberEndpoints.Map(group);
        Tickets.GamingTicketEndpoints.Map(group);
        
        MapRedeemEndpoints(group);
    }





    private static void MapRedeemEndpoints(RouteGroupBuilder group)
    {
        group.MapPost(
                "/prizes/awards/{awardId:guid}/redeem",
                async (Guid awardId, RedeemPrizeAwardRequest request, ISender sender, CancellationToken ct) =>
                {
                    RedeemPrizeAwardCommand command = new RedeemPrizeAwardCommand(awardId, request.Note);
                    return await UseCaseInvoker.Send<RedeemPrizeAwardCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RedeemPrizeAward");
    }
}
