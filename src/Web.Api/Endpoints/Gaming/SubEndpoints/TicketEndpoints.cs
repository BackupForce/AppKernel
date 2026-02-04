using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Cancel;
using Application.Gaming.Tickets.Claim;
using Application.Gaming.Tickets.Issue;
using Application.Gaming.Tickets.Redeem;
using Application.Gaming.Tickets.Submit;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.SubEndpoints;

internal static class TicketEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        RouteGroupBuilder ticketGroup = group.MapGroup("/tickets")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Gaming.Tickets");

        ticketGroup.MapPost(
                "/issue",
                async (IssueTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new IssueTicketCommand(
                        request.MemberId,
                        request.ResolveDrawGroupId(),
                        request.TicketTemplateId,
                        request.IssuedReason);
                    return await UseCaseInvoker.Send<IssueTicketCommand, IssueTicketResult>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .Produces<IssueTicketResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("IssueTicket");

        ticketGroup.MapPost(
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



        ticketGroup.MapPost(
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

        ticketGroup.MapPost(
                "/{ticketId:guid}/cancel",
                async (Guid ticketId, CancelTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CancelTicketCommand(ticketId, request.Reason);
                    return await UseCaseInvoker.Send(
                        command,
                        sender,
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CancelTicket");
    }
}
