using Application.Users.RemoveRole;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class RemoveUserRoleEndpoint
{
    public static RouteHandlerBuilder MapRemoveUserRoleEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapDelete(
                "/{userId:guid}/roles/{roleId:int}",
                async (Guid userId, int roleId, ISender sender, CancellationToken ct) =>
                {
                    RemoveUserRoleCommand command = new RemoveUserRoleCommand(userId, roleId);
                    Result<RemoveUserRoleResultDto> result = await sender.Send(command, ct);
                    return result.Match(
                        _ => Results.NoContent(),
                        failure => CustomResults.Problem(failure));
                })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("RemoveUserRole");
    }
}
