using Application.Roles.Dtos;
using Application.Roles.List;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class GetRolesEndpoint
{
    public static RouteHandlerBuilder MapGetRolesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async (ISender sender, CancellationToken ct) =>
                {
                    var query = new ListRolesQuery();
                    return await UseCaseInvoker.Send<ListRolesQuery, IReadOnlyList<RoleListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<IReadOnlyList<RoleListItemDto>>(StatusCodes.Status200OK)
            .WithName("ListRoles");
    }
}
