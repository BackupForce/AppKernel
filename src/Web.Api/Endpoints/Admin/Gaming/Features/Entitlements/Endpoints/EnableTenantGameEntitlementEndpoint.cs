using Application.Abstractions.Authorization;
using Application.Abstractions.Gaming;
using Application.Gaming.Entitlements;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Entitlements.Endpoints;

public static class EnableTenantGameEntitlementEndpoint
{
    public static RouteHandlerBuilder MapEnableTenantGameEntitlementEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPatch(
                "/games/{gameCode}/enable",
                async (Guid tenantId, string gameCode, ISender sender, CancellationToken ct) =>
                {
                    var command = new EnableTenantGameEntitlementCommand(tenantId, gameCode);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .WithName("EnableTenantGameEntitlement");
    }
}
