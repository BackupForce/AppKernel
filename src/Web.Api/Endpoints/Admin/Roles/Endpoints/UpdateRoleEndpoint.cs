using Application.Roles.Update;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class UpdateRoleEndpoint
{
    public static RouteHandlerBuilder MapUpdateRoleEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{id:int}",
                async (int id, UpdateRoleRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateRoleCommand(id, request.Name);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Roles.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateRole");
    }
}
