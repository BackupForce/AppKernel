using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Admin.Dashboard;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Dashboard.Endpoints;

public static class GetOnlineMembersEndpoint
{
    public static RouteHandlerBuilder MapGetOnlineMembersEndpoint(
        this RouteGroupBuilder group)
    {

        return group.MapGet(
                "/online-members",
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
