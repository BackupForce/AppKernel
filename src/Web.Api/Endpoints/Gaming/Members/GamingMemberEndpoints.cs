using Application.Abstractions.Authorization;
using Application.Gaming.Awards.GetMy;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.GetMy;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.Members;

internal static class GamingMemberEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
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
    }
}
