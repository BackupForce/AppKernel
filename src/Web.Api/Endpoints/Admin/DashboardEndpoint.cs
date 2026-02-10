using Application.Abstractions.Authorization;
using Application.Admin.Dashboard;
using Application.Users.RemoveRole;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Admin;

public sealed class DashboardEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
     
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Users");

        group.MapGet(
                "/dashboard/member-metrics",
               async (ISender sender, CancellationToken ct) =>
                    await UseCaseInvoker.Send<GetAdminDashboardMemberMetricsQuery, AdminDashboardMemberMetricsDto>(
                        new GetAdminDashboardMemberMetricsQuery(),
                        sender,
                        Results.Ok,
                        ct))
            .RequireAuthorization(AuthorizationPolicyNames.AdminPolicy)
            .Produces<AdminDashboardMemberMetricsDto>(StatusCodes.Status200OK)
            .WithName("GetAdminDashboardMemberMetrics");
    }
}
