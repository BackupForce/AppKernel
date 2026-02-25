using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Asp.Versioning;
using Domain.Users;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Admin;

public sealed class GetMyPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin/me")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Me");

        group.MapGet(
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

    public sealed record GetMyPermissionsRequest(Guid? NodeId);

    public sealed record GetMyPermissionsResponse(IReadOnlyList<string> Items);
}
