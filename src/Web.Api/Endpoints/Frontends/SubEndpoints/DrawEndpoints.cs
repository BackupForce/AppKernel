using Application.Abstractions.Authorization;
using Application.Gaming.Draws.GetCurrentDrawHotBalls;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Frontends.SubEndpoints;

internal static class DrawEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/gaming/draws")
            .WithTags("FE.Draws");

        group.MapGet(
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
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces(StatusCodes.Status200OK)
            .WithSummary("當前期別熱球統計")
            .WithDescription("取得當前期別每顆球號的下注次數排名。")
            .WithName("GetCurrentDrawHotBalls");
    }
}
