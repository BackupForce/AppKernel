using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Admin.Dashboard;
using Application.Users.RemoveRole;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Dashboard.Endpoints;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Dashboard;

internal static class DashboardEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/dashboard")
            .WithTags("Admin.Dashboard");

        group.MapGetDashboardMemberMetricsEndpoint();
        group.MapGetOnlineMembersEndpoint();
    }
}
