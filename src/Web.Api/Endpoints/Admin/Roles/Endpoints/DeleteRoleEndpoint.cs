using Application.Roles.Delete;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class DeleteRoleEndpoint
{
    public static RouteHandlerBuilder MapDeleteRoleEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapDelete(
                "/{id:int}",
                async (int id, ISender sender, CancellationToken ct) =>
                {
                    var command = new DeleteRoleCommand(id);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Roles.Delete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteRole");
    }
}
