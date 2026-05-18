using Application.Roles.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class CreateRoleEndpoint
{
    public static RouteHandlerBuilder MapCreateRoleEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (CreateRoleRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreateRoleCommand(request.Name);
                    return await UseCaseInvoker.Send<CreateRoleCommand, int>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.Create.Name)
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateRole");
    }
}
