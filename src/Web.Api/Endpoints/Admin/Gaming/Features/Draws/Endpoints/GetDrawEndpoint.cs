using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetDrawEndpoint
{
    public static RouteHandlerBuilder MapGetDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawId:guid}",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawByIdQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawByIdQuery, DrawDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<DrawDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetGameDrawById");
    }
}
