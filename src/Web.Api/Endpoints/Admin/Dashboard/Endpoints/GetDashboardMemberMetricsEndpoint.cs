using Application.Abstractions.Authorization;
using Application.Admin.Dashboard;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Dashboard.Endpoints;

public static class GetDashboardMemberMetricsEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardMemberMetricsEndpoint(
        this RouteGroupBuilder group)
    {

        return group.MapGet(
                "/member-metrics",
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
