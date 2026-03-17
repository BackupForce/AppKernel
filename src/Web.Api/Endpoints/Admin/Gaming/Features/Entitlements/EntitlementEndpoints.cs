using Web.Api.Endpoints.Admin.Gaming.Features.Entitlements.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Entitlements;

internal static class EntitlementEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/entitlements")
            .WithTags("Gaming.Entitlements");

        group.MapGetTenantEntitlementsEndpoint();

        group.MapEnableTenantGameEntitlementEndpoint();
        group.MapDisableTenantGameEntitlementEndpoint();

        group.MapEnableTenantPlayEntitlementEndpoint();
        group.MapDisableTenantPlayEntitlementEndpoint();

    }
}
