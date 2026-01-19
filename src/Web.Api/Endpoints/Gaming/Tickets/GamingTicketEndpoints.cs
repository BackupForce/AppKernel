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

namespace Web.Api.Endpoints.Gaming.Tickets;

internal static class GamingTicketEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        RouteGroupBuilder ticketGroup = group.MapGroup("/tickets")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Gaming Tickets");

        ticketGroup.MapPost(
                "/issue",
                async (IssueTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    IssueTicketCommand command = new IssueTicketCommand(
                        request.MemberId,
                        request.CampaignId,
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
                "/campaigns/{campaignId:guid}/claim",
                async (Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    ClaimCampaignTicketCommand command = new ClaimCampaignTicketCommand(campaignId);
                    return await UseCaseInvoker.Send<ClaimCampaignTicketCommand, IssueTicketResult>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IssueTicketResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ClaimCampaignTicket");

        ticketGroup.MapPost(
                "/{ticketId:guid}/submit",
                async (Guid ticketId, SubmitTicketNumbersRequest request, ISender sender, CancellationToken ct) =>
                {
                    SubmitTicketNumbersCommand command =
                        new(ticketId, request.Numbers);

                    return await UseCaseInvoker.Send(
                        command,
                        sender,
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SubmitTicketNumbers");

        ticketGroup.MapPost(
                "/{ticketId:guid}/draws/{drawId:guid}/redeem",
                async (Guid ticketId, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    RedeemTicketDrawCommand command = new RedeemTicketDrawCommand(ticketId, drawId);
                    return await UseCaseInvoker.Send<RedeemTicketDrawCommand>(
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
                    CancelTicketCommand command = new CancelTicketCommand(ticketId, request.Reason);
                    return await UseCaseInvoker.Send<CancelTicketCommand>(
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
