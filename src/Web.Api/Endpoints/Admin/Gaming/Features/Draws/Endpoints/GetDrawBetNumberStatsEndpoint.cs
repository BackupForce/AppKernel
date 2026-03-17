using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetDrawBetNumberStats;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetDrawBetNumberStatsEndpoint
{
    public static RouteHandlerBuilder MapGetDrawBetNumberStatsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawId:guid}/bet-number-stats",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawBetNumberStatsQuery query = new(drawId);
                    return await UseCaseInvoker.Send<GetDrawBetNumberStatsQuery, IReadOnlyCollection<DrawBetNumberStatDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.WinningNumbersRead.Name)
            .Produces<IReadOnlyCollection<DrawBetNumberStatDto>>(StatusCodes.Status200OK)
            .WithName("AdminGetDrawBetNumberStats");
    }
}
