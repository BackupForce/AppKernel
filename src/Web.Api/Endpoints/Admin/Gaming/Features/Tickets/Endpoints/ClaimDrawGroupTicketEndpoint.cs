using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Claim;
using Application.Gaming.Tickets.Issue;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

public static class ClaimDrawGroupTicketEndpoint
{
    public static RouteHandlerBuilder MapClaimDrawGroupTicketEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/drawgroups/{drawgroupId:guid}/claim",
                async (Guid drawgroupId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ClaimDrawGroupTicketCommand(drawgroupId);
                    return await UseCaseInvoker.Send<ClaimDrawGroupTicketCommand, IssueTicketResult>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IssueTicketResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ClaimDrawGroupTicket");
    }
}
