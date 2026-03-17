using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Domain.Users;
using Web.Api.Endpoints.Admin.Me.Requests;
using Web.Api.Endpoints.Admin.Me.Responses;

namespace Web.Api.Endpoints.Admin.Me.Endpoints;
public static class GetMyPermissionsEndpoint
{
    public static RouteHandlerBuilder MapGetMyPermissionsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/permissions",
                async (
                    ClaimsPrincipal user,
                    [AsParameters] GetMyPermissionsRequest request,
                    ITenantContext tenantContext,
                    IPermissionProvider permissionProvider,
                    CancellationToken ct) =>
                {
                    if (!JwtUserContext.TryFromClaims(user, out JwtUserContext? jwtContext) ||
                        jwtContext is null ||
                        jwtContext.UserType != UserType.Tenant)
                    {
                        return Results.Forbid();
                    }

                    bool hasTenantContext = tenantContext.TryGetTenantId(out Guid resolvedTenantId);
                    if (jwtContext.TenantId.HasValue && hasTenantContext && jwtContext.TenantId.Value != resolvedTenantId)
                    {
                        return Results.Forbid();
                    }

                    Guid tenantId = jwtContext.TenantId ?? (hasTenantContext ? resolvedTenantId : Guid.Empty);
                    if (tenantId == Guid.Empty)
                    {
                        return Results.Forbid();
                    }

                    IReadOnlyList<string> codes = await permissionProvider.GetAllowedPermissionCodesAsync(
                        jwtContext.UserId,
                        tenantId,
                        request.NodeId,
                        ct);

                    return Results.Ok(new GetMyPermissionsResponse(codes));
                })
            .Produces<GetMyPermissionsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("AdminGetMyPermissions");
    }
}
