using Application.Abstractions.Authorization;
using Application.Abstractions.Gaming;
using Application.Gaming.Entitlements;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Gaming.Entitlements;

internal static class GamingEntitlementEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(
                "/entitlements",
                async (Guid tenantId, ISender sender, CancellationToken ct) =>
                {
                    GetTenantEntitlementsQuery query = new GetTenantEntitlementsQuery(tenantId);
                    return await UseCaseInvoker.Send<GetTenantEntitlementsQuery, TenantEntitlementsDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .Produces<TenantEntitlementsDto>(StatusCodes.Status200OK)
            .WithName("GetTenantEntitlements");

        group.MapPatch(
                "/entitlements/games/{gameCode}/enable",
                async (Guid tenantId, string gameCode, ISender sender, CancellationToken ct) =>
                {
                    EnableTenantGameEntitlementCommand command = new EnableTenantGameEntitlementCommand(tenantId, gameCode);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .WithName("EnableTenantGameEntitlement");

        group.MapPatch(
                "/entitlements/games/{gameCode}/disable",
                async (Guid tenantId, string gameCode, ISender sender, CancellationToken ct) =>
                {
                    DisableTenantGameEntitlementCommand command = new DisableTenantGameEntitlementCommand(tenantId, gameCode);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .WithName("DisableTenantGameEntitlement");

        group.MapPatch(
                "/entitlements/games/{gameCode}/plays/{playTypeCode}/enable",
                async (Guid tenantId, string gameCode, string playTypeCode, ISender sender, CancellationToken ct) =>
                {
                    EnableTenantPlayEntitlementCommand command = new EnableTenantPlayEntitlementCommand(
                        tenantId,
                        gameCode,
                        playTypeCode);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .WithName("EnableTenantPlayEntitlement");

        group.MapPatch(
                "/entitlements/games/{gameCode}/plays/{playTypeCode}/disable",
                async (Guid tenantId, string gameCode, string playTypeCode, ISender sender, CancellationToken ct) =>
                {
                    DisableTenantPlayEntitlementCommand command = new DisableTenantPlayEntitlementCommand(
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
