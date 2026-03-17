using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin.Winnings;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Winnings.Endpoints;

public static class RedeemWinningEndpoint
{
    public static RouteHandlerBuilder MapRedeemWinningEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{winningId:guid}/redeem",
                async (Guid winningId, ISender sender, CancellationToken ct) =>
                {
                    RedeemWinningCommand command = new RedeemWinningCommand(winningId);
                    return await UseCaseInvoker.Send<RedeemWinningCommand, AdminWinningDetailDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.WinningsRedeem.Name)
            .Produces<AdminWinningDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("AdminRedeemWinning");
    }
}
