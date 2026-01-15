using Application.Gaming.Catalog;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Gaming.Catalog;

internal static class GamingCatalogEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(
                "/catalog/games",
                async (ISender sender, CancellationToken ct) =>
                {
                    GetGameCatalogQuery query = new GetGameCatalogQuery();
                    return await UseCaseInvoker.Send<GetGameCatalogQuery, IReadOnlyCollection<GameCatalogDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.CatalogView.Name)
            .Produces<IReadOnlyCollection<GameCatalogDto>>(StatusCodes.Status200OK)
            .WithName("GetGameCatalog");
    }
}
