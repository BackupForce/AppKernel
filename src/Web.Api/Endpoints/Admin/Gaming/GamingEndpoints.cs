using Application.Abstractions.Authorization;
using Asp.Versioning;
using Web.Api.Endpoints.Admin.Gaming.Features.Catalogs;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawTemplates;
using Web.Api.Endpoints.Admin.Gaming.Features.Entitlements;
using Web.Api.Endpoints.Admin.Gaming.Features.Prizes;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates;
using Web.Api.Endpoints.Admin.Gaming.Features.Winnings;

namespace Web.Api.Endpoints.Admin.Gaming;

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
            .WithGroupName("gaming-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Gaming");

        DrawEndpoints.Map(group);
        DrawTemplateEndpoints.Map(group);
        DrawGroupEndpoints.Map(group);
        
        PrizeEndpoints.Map(group);
        
        EntitlementEndpoints.Map(group);
        
        CatalogEndpoints.Map(group);

        TicketEndpoints.Map(group);
        TicketTemplateEndpoints.Map(group);
        TicketClaimEventEndpoints.Map(group);

        WinningsEndpoints.Map(group);
    }
}
