using Application.Abstractions.Gaming;
using Application.Gaming.Entitlements;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Entitlements.Endpoints;

public static class GetTenantEntitlementsEndpoint
{
    public static RouteHandlerBuilder MapGetTenantEntitlementsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
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
    }
}
