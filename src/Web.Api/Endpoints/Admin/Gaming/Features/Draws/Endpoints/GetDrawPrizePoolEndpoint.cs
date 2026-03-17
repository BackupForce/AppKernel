using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.PrizePool;
using Application.Gaming.Draws.PrizePool.Get;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetDrawPrizePoolEndpoint
{
    public static RouteHandlerBuilder MapGetDrawPrizePoolEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawId:guid}/prize-pool",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawPrizePoolQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawPrizePoolQuery, DrawPrizePoolDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces<DrawPrizePoolDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("GetGameDrawPrizePool");
    }
}
