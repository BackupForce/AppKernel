using Application.Admin.Dashboard;
using Application.Abstractions.Authorization;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Admin;

public sealed class DashboardEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/admin-v1/dashboard/member-metrics",
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
