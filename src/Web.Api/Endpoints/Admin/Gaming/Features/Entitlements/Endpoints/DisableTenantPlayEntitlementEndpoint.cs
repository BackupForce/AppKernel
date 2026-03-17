using Application.Abstractions.Authorization;
using Application.Abstractions.Gaming;
using Application.Gaming.Entitlements;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Entitlements.Endpoints;

public static class DisableTenantPlayEntitlementEndpoint
{
    public static RouteHandlerBuilder MapDisableTenantPlayEntitlementEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPatch(
                "/games/{gameCode}/plays/{playTypeCode}/disable",
                async (Guid tenantId, string gameCode, string playTypeCode, ISender sender, CancellationToken ct) =>
                {
                    var command = new DisableTenantPlayEntitlementCommand(
                        tenantId,
                        gameCode,
                        playTypeCode);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .WithName("DisableTenantPlayEntitlement");
    }
}
