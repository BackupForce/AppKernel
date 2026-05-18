using Application.Abstractions.Data;
using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.List;
using Application.Gaming.DrawGroups.RemoteSearch;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class GetDrawGroupsEndpoint
{
    public static RouteHandlerBuilder MapGetDrawGroupsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] ListDrawGroupsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new ListDrawGroupsQuery(
                        tenantId,
                        request.Status,
                        request.GameCode,
                        request.Keyword,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<ListDrawGroupsQuery, PagedResult<DrawGroupSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingDrawGroup.Read.Name)
            .Produces<PagedResult<DrawGroupSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ListDrawGroups");
    }
}
