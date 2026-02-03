using Application.Abstractions.Authorization;
using Application.Abstractions.Gaming;
using Application.Gaming.Entitlements;
using Domain.Security;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;

namespace Web.Api.Endpoints.Gaming.SubEndpoints;

internal static class EntitlementEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/entitlements")
            .WithTags("Gaming.Entitlements");

        group.MapGet(
                "",
                async (Guid tenantId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetTenantEntitlementsQuery(tenantId);
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

        group.MapPatch(
                "/games/{gameCode}/disable",
                async (Guid tenantId, string gameCode, ISender sender, CancellationToken ct) =>
                {
                    var command = new DisableTenantGameEntitlementCommand(tenantId, gameCode);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.EntitlementManage.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .WithName("DisableTenantGameEntitlement");

        group.MapPatch(
                "/games/{gameCode}/plays/{playTypeCode}/enable",
                async (Guid tenantId, string gameCode, string playTypeCode, ISender sender, CancellationToken ct) =>
                {
                    var command = new EnableTenantPlayEntitlementCommand(
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
