using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Gaming.Awards.GetMy;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claim;
using Application.Gaming.Tickets.AvailableForBet;
using Application.Gaming.Tickets.GetMy;
using Domain.Gaming.Shared;
using Domain.Members;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Gaming.Members;

internal static class GamingMemberEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(
                "/members/me/ticket-claim-events/{eventId:guid}/claim",
                async (Guid eventId,
                    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    ClaimTicketFromEventCommand command = new(eventId, idempotencyKey);
                    return await UseCaseInvoker.Send<ClaimTicketFromEventCommand, TicketClaimResult>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<TicketClaimResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ClaimTicketFromEvent");

        group.MapGet(
                "/games/{gameCode}/members/me/tickets",
                async (string gameCode, [AsParameters] GetMyTicketsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMyTicketsQuery query = new GetMyTicketsQuery(gameCode, request.From, request.To);
                    return await UseCaseInvoker.Send<GetMyTicketsQuery, IReadOnlyCollection<TicketSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IReadOnlyCollection<TicketSummaryDto>>(StatusCodes.Status200OK)
            .WithName("GetMyGameTickets");

        group.MapGet(
                "/games/{gameCode}/members/me/awards",
                async (string gameCode, [AsParameters] GetMyAwardsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMyAwardsQuery query = new GetMyAwardsQuery(gameCode, request.Status);
                    return await UseCaseInvoker.Send<GetMyAwardsQuery, IReadOnlyCollection<PrizeAwardDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IReadOnlyCollection<PrizeAwardDto>>(StatusCodes.Status200OK)
            .WithName("GetMyGameAwards");

        group.MapGet(
                "/members/me/tickets/available-for-bet",
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

                    GetAvailableTicketsForBetQuery query = new GetAvailableTicketsForBetQuery(
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
            .WithName("GetAvailableTicketsForBet");
    }
}
