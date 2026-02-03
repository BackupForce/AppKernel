using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claim;
using Application.Gaming.Tickets.AvailableForBet;
using Application.Gaming.Tickets.GetMy;
using Application.Gaming.Tickets.Place;
using Domain.Gaming.Shared;
using Domain.Members;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Frontends.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Frontends.SubEndpoints;

internal static class TicketEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/tickets")
            .WithTags("FE.Tickets");


        group.MapGet(
                "/available-for-bet",
                async ([AsParameters] GetAvailableTicketsForBetRequest request,
                    IMemberRepository memberRepository,
                    ITenantContext tenantContext,
                    IUserContext userContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    Member? member = await memberRepository.GetByUserIdAsync(
                        tenantContext.TenantId,
                        userContext.UserId,
                        ct);
                    if (member is null)
                    {
                        return CustomResults.Problem(Result.Failure(GamingErrors.MemberNotFound));
                    }

                    var query = new GetAvailableTicketsForBetQuery(
                        tenantContext.TenantId,
                        member.Id,
                        request.DrawId,
                        request.Limit);
                    return await UseCaseInvoker.Send<GetAvailableTicketsForBetQuery, AvailableTicketsResponse>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<AvailableTicketsResponse>(StatusCodes.Status200OK)
            .WithSummary("可供參與票券")
            .WithDescription("取得未參與票券清單")
            .WithName("GetAvailableTicketsForBet");

        group.MapGet(
                "/",
                async ([AsParameters] GetMyTicketsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetMyTicketsQuery(request.GameCode, request.From, request.To);
                    return await UseCaseInvoker.Send<GetMyTicketsQuery, IReadOnlyCollection<TicketSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IReadOnlyCollection<TicketSummaryDto>>(StatusCodes.Status200OK)
            .WithName("GetMyGameTickets");


        group.MapPost(
                "/",
                async (PlaceTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new PlaceTicketCommand(
                        request.DrawId,
                        request.PlayTypeCode,
                        request.TemplateId,
                        request.Lines);
                    return await UseCaseInvoker.Send<PlaceTicketCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("PlaceGameTicket");


    }
}
