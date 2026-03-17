using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.PrizePool;
using Application.Gaming.Draws.PrizePool.Update;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class UpdateDrawPrizePoolEndpoint
{
    public static RouteHandlerBuilder MapUpdateDrawPrizePoolEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{drawId:guid}/prize-pool",
                async (Guid drawId, UpdateDrawPrizePoolRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateDrawPrizePoolCommand(
                        drawId,
                        request.Items.Select(item => new UpdateDrawPrizePoolItem(
                            item.PlayTypeCode,
                            item.Tier,
                            new PrizeOptionDto(
                                item.Option.PrizeId,
                                item.Option.Name,
                                item.Option.Cost,
                                item.Option.PayoutAmount,
                                item.Option.RedeemValidDays,
                                item.Option.Description))).ToList());
                    return await UseCaseInvoker.Send<UpdateDrawPrizePoolCommand, DrawPrizePoolDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces<DrawPrizePoolDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateGameDrawPrizePool");
    }
}
