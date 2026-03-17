using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Draws.AllowedTicketTemplates.Get;
using Application.Gaming.Draws.AllowedTicketTemplates.Update;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetCurrentDrawHotBalls;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.ManualClose;
using Application.Gaming.Draws.PrizePool;
using Application.Gaming.Draws.PrizePool.Get;
using Application.Gaming.Draws.PrizePool.Update;
using Application.Gaming.Draws.PrizePool.Validate;
using Application.Gaming.Draws.RemoteSearch;
using Application.Gaming.Draws.Reopen;
using Application.Gaming.Draws.SellingOptions;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Place;
using Domain.Security;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws;

internal static class DrawEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent
            .MapGroup("/draws")
            .WithTags("Gaming.Draws");

        group.MapCreateDrawEndpoint();
        group.MapRemoteSearchDrawsEndpoint();
        group.MapGetDrawsEndpoint();
        group.MapGetDrawEndpoint();

        group.MapGetSellingDrawOptionsEndpoint();

        //Actions
        group.MapExecuteDrawEndpoint();
        group.MapSettleDrawEndpoint();
        group.MapCloseDrawManuallyEndpoint();
        group.MapReopenDrawEndpoint();

        //AllowedTicketTemplates
        group.MapGetDrawAllowedTicketTemplatesEndpoint();
        group.MapUpdateDrawAllowedTicketTemplatesEndpoint();

        //PrizePool
        group.MapGetDrawPrizePoolEndpoint();
        group.MapUpdateDrawPrizePoolEndpoint();
        group.MapValidateDrawPrizePoolEndpoint();

        //HotBalls
        group.MapGetCurrentDrawHotBallsEndpoint();


        group.MapSetDrawWinningNumbersEndpoint();

        //Tickets
        group.MapGetDrawTicketsEndpoint();
    }
}
