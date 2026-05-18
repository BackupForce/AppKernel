using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetCurrentDrawHotBalls;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetCurrentDrawHotBallsEndpoint
{
    public static RouteHandlerBuilder MapGetCurrentDrawHotBallsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
               "/current/hot-balls",
               async (ISender sender, CancellationToken ct) =>
               {
                   GetCurrentDrawHotBallsQuery query = new();

                   return await UseCaseInvoker.Send<GetCurrentDrawHotBallsQuery, IReadOnlyCollection<HotBallDto>>(
                       query,
                       sender,
                       value => Results.Ok(new { items = value }),
                       ct);
               })
           .Produces(StatusCodes.Status200OK)
           .WithSummary("當前期別熱球統計")
           .WithDescription("取得當前期別每顆球號的下注次數排名。")
           .WithName("GetCurrentDrawHotBalls");
    }
}
