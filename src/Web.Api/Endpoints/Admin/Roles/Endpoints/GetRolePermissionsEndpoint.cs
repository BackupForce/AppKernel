using Application.Roles.Dtos;
using Application.Roles.Permissions;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class GetRolePermissionsEndpoint
{
    public static RouteHandlerBuilder MapGetRolePermissionsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{id:int}/permissions",
                async (int id, ISender sender, CancellationToken ct) =>
                {
                    var request = new GetRolePermissionsQuery(id);
                    return await UseCaseInvoker.Send<GetRolePermissionsQuery, IReadOnlyList<string>>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetRolePermissions");
    }
}
