using Application.Abstractions.Data;
using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.RemoteSearch;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class RemoteSearchDrawGroupsEndpoint
{
    public static RouteHandlerBuilder MapRemoteSearchDrawGroupsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/remote-search",
                async (Guid tenantId, [AsParameters] RemoteSearchDrawGroupsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new RemoteSearchDrawGroupsQuery(
                        tenantId,
                        request.Q,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<RemoteSearchDrawGroupsQuery, PagedResult<DrawGroupRemoteSearchDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingDrawGroup.Read.Name)
            .Produces<PagedResult<DrawGroupRemoteSearchDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoteSearchDrawGroups");
    }
}
