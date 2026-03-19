using Application.Roles.Permissions;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class RemoveRolePermissionsEndpoint
{
    public static RouteHandlerBuilder MapRemoveRolePermissionsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
            "/{id:int}/permissions/remove",
            async (int id, UpdateRolePermissionsRequest request, ISender sender, CancellationToken ct) =>
            {
                var command = new RemoveRolePermissionsCommand(id, request.PermissionCodes);
                return await UseCaseInvoker.Send(command, sender, ct);
            })
        .RequireAuthorization(Permission.Roles.Update.Name)
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithName("RemoveRolePermissions");
    }
}
