using Application.Abstractions.Data;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.RemoteSearch;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class RemoteSearchDrawsEndpoint
{
    public static RouteHandlerBuilder MapRemoteSearchDrawsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/remote-search",
                async (Guid tenantId, [AsParameters] RemoteSearchDrawsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new RemoteSearchDrawsQuery(
                        tenantId,
                        request.Q,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<RemoteSearchDrawsQuery, PagedResult<DrawRemoteSearchDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingDrawGroup.Read.Name)
            .Produces<PagedResult<DrawRemoteSearchDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoteSearchDraws");
    }
}
