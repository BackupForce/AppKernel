using Application.Gaming.Dtos;
using Application.Gaming.Prizes.GetList;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Endpoints;

public static class GetPrizesEndpoint
{
    public static RouteHandlerBuilder MapGetPrizesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async (ISender sender, CancellationToken ct) =>
                {
                    var query = new GetPrizeListQuery();
                    return await UseCaseInvoker.Send<GetPrizeListQuery, IReadOnlyCollection<PrizeDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<PrizeDto>>(StatusCodes.Status200OK)
            .WithName("GetPrizeList");
    }
}
