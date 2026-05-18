using Application.Abstractions.Authorization;
using Application.Gaming.Awards.GetMy;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claim;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Frontends.Requests;

namespace Web.Api.Endpoints.Frontends.SubEndpoints;


internal static class AwardEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/awards")
            .WithTags("FE.Awards");


        group.MapGet(
          "/",
          async ([AsParameters] GetMyAwardsRequest request, ISender sender, CancellationToken ct) =>
              {
                  var query = new GetMyAwardsQuery(request.GameCode, request.Status);
                  return await UseCaseInvoker.Send<GetMyAwardsQuery, IReadOnlyCollection<PrizeAwardDto>>(
                      query,
                      sender,
                      value => Results.Ok(value),
                      ct);
              })
          .RequireAuthorization(AuthorizationPolicyNames.Member)
          .Produces<IReadOnlyCollection<PrizeAwardDto>>(StatusCodes.Status200OK)
          .WithSummary("取得獎項")
          .WithDescription("取得獎項")
          .WithName("GetMyGameAwards");
    }
}
