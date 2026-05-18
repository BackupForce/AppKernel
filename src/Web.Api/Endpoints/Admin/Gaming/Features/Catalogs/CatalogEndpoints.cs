using Application.Gaming.Catalog;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Catalogs;

internal static class CatalogEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/catalog")
            .WithTags("Gaming.Catalogs");

        group.MapGet(
                "/games",
                async (ISender sender, CancellationToken ct) =>
                {
                    var query = new GetGameCatalogQuery();
                    return await UseCaseInvoker.Send<GetGameCatalogQuery, IReadOnlyCollection<GameCatalogDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.CatalogView.Name)
            .Produces<IReadOnlyCollection<GameCatalogDto>>(StatusCodes.Status200OK)
            .WithSummary("取得遊戲玩法")
            .WithDescription("取得遊戲玩法")
            .WithName("GetGameCatalog");
    }
}
