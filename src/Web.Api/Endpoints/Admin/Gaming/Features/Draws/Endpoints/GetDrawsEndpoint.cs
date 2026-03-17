using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetDrawsEndpoint
{
    public static RouteHandlerBuilder MapGetDrawsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async ([AsParameters] GetDrawsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetOpenDrawsQuery(request.Status);
                    return await UseCaseInvoker.Send<GetOpenDrawsQuery, IReadOnlyCollection<DrawSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<IReadOnlyCollection<DrawSummaryDto>>(StatusCodes.Status200OK)
            .WithName("GetGameOpenDraws");
    }
}
