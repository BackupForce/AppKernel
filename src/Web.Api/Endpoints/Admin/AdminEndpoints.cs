using Application.Abstractions.Authorization;
using Application.Authorization;
using Asp.Versioning;
using Web.Api.Endpoints.Admin.Dashboard;
using Web.Api.Endpoints.Admin.Me;
using Web.Api.Endpoints.Admin.Roles;
using Web.Api.Endpoints.Admin.Users;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser);

        UsersEndpoints.Map(group);
        RolesEndpoints.Map(group);
        DashboardEndpoints.Map(group);
        MeEndpoints.Map(group);
    }
}
