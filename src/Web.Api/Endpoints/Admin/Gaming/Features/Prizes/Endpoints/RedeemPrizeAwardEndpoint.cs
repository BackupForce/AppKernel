using Application.Abstractions.Authorization;
using Application.Gaming.Awards.Redeem;
using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.GetList;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Endpoints;

public static class RedeemPrizeAwardEndpoint
{
    public static RouteHandlerBuilder MapRedeemPrizeAwardEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/awards/{awardId:guid}/redeem",
                async (Guid awardId, RedeemPrizeAwardRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new RedeemPrizeAwardCommand(awardId, request.Note);
                    return await UseCaseInvoker.Send<RedeemPrizeAwardCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RedeemPrizeAward");
    }
}
