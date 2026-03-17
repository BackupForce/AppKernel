using Application.Abstractions.Data;
using Application.Users.AssignRole;
using Application.Users.Create;
using Application.Users.GetTenantUsers;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;
using Web.Api.Endpoints.Users.Requests;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class AssignRoleToUserEndpoint
{
    public static RouteHandlerBuilder MapAssignRoleToUserEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{userId:guid}/roles/{roleId:int}",
                async (Guid userId, int roleId, ISender sender, CancellationToken ct) =>
                {
                    AssignRoleToUserCommand command = new AssignRoleToUserCommand(userId, roleId);
                    return await UseCaseInvoker.Send<AssignRoleToUserCommand, AssignRoleToUserResultDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces<AssignRoleToUserResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("AssignRoleToUser");
    }
}
