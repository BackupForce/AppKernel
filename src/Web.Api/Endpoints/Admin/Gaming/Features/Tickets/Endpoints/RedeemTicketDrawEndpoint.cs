using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Issue;
using Application.Gaming.Tickets.Redeem;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

public static class RedeemTicketDrawEndpoint
{
    public static RouteHandlerBuilder MapRedeemTicketDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{ticketId:guid}/draws/{drawId:guid}/redeem",
                async (Guid ticketId, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new RedeemTicketDrawCommand(ticketId, drawId);
                    return await UseCaseInvoker.Send(
                        command,
                        sender,
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RedeemTicketDraw");
    }
}
