using Application.Abstractions.Data;
using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.GetById;
using Application.Gaming.DrawGroups.List;
using Application.Gaming.DrawGroups.RemoteSearch;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class GetDrawGroupEndpoint
{
    public static RouteHandlerBuilder MapGetDrawGroupEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawGroupId:guid}",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawGroupByIdQuery(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send<GetDrawGroupByIdQuery, DrawGroupDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<DrawGroupDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetDrawGroupById");
    }
}
