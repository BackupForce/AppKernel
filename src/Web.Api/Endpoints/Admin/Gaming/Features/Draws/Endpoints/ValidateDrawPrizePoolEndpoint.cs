using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.PrizePool;
using Application.Gaming.Draws.PrizePool.Validate;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class ValidateDrawPrizePoolEndpoint
{
    public static RouteHandlerBuilder MapValidateDrawPrizePoolEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{drawId:guid}/prize-pool/validation",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new ValidateDrawPrizePoolQuery(drawId);
                    return await UseCaseInvoker.Send<ValidateDrawPrizePoolQuery, DrawPrizePoolValidationDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces<DrawPrizePoolValidationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ValidateGameDrawPrizePool");
    }
}
