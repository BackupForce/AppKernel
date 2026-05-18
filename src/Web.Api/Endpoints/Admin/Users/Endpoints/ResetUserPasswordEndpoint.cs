using Application.Users.ResetPassword;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Users.Requests;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class ResetUserPasswordEndpoint
{
    public static RouteHandlerBuilder MapResetUserPasswordEndpoint(this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{id:guid}/reset-password",
                async (Guid id, ResetUserPasswordRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new ResetUserPasswordCommand(id, request.NewPassword);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Users.ResetPassword.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("ResetUserPassword");
    }
}
