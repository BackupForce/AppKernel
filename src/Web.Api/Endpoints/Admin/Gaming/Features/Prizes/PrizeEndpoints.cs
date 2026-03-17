using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.Deactivate;
using Application.Gaming.Prizes.GetList;
using Application.Gaming.Prizes.Update;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes;

internal static class PrizeEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/prizes")
            .WithTags("Gaming.Prizes");

        //CRUD
        group.MapCreatePrizeEndpoint();
        group.MapGetPrizesEndpoint();
        group.MapUpdatePrizeEndpoint();
        
        //Action
        group.MapActivatePrizeEndpoint();
        group.MapDeactivatePrizeEndpoint();

        //TODO: 思考一下
        group.MapRedeemPrizeAwardEndpoint();
    }

}
