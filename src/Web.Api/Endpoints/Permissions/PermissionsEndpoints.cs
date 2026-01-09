using Application.Authorization;
using Asp.Versioning;

namespace Web.Api.Endpoints.Permissions;

public sealed class PermissionsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/permissions")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization()
            .WithTags("Permissions");

        // 中文註解：提供前端取得 UI 友善的權限目錄。
        group.MapGet(
                "/catalog",
                (PermissionUiCatalogProvider provider) =>
                {
                    PermissionCatalogDto catalog = PermissionUiCatalogProvider.GetCatalog();
                    return Results.Ok(catalog);
                })
            .Produces<PermissionCatalogDto>(StatusCodes.Status200OK)
            .WithName("GetPermissionCatalog");
    }
}
