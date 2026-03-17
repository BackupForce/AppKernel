using Application.Users.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class CreateUserEndpoint
{
    public static RouteHandlerBuilder MapCreateUserEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (CreateTenantUserRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateUserCommand command = new CreateUserCommand(
                        request.Email,
                        request.Name,
                        request.Password,
                        request.HasPublicProfile,
                        null,
                        null);
                    return await UseCaseInvoker.Send<CreateUserCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.Create.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateTenantUser");
    }
}
