using Application.Abstractions.Authorization;
using Application.Admin.Dashboard;
using Application.Users.RemoveRole;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints;
using Web.Api.Endpoints.Admin.Requests;

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

        app.MapGet(
                "/admin-v1/dashboard/online-members",
                async ([AsParameters] GetOnlineMembersRequest request, ISender sender, CancellationToken ct) =>
                {
                    if (request.Page < 1)
                    {
                        return Results.BadRequest("Page must be greater than or equal to 1.");
                    }

                    if (request.PageSize < 1 || request.PageSize > 200)
                    {
                        return Results.BadRequest("PageSize must be between 1 and 200.");
                    }

                    if (request.WindowMinutes < 1 || request.WindowMinutes > 60)
                    {
                        return Results.BadRequest("WindowMinutes must be between 1 and 60.");
                    }

                    GetOnlineMembersQuery query = new(
                        request.Page,
                        request.PageSize,
                        request.WindowMinutes,
                        request.Q);

                    return await UseCaseInvoker.Send<GetOnlineMembersQuery, PagedResponse<OnlineMemberDto>>(
                        query,
                        sender,
                        Results.Ok,
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.AdminPolicy)
            .Produces<PagedResponse<OnlineMemberDto>>(StatusCodes.Status200OK)
            .WithName("GetOnlineMembers");
    }
}
