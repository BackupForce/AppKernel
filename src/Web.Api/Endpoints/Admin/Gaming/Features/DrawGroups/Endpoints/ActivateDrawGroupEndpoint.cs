using Application.Gaming.DrawGroups.Activate;
using Application.Gaming.DrawGroups.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class ActivateDrawGroupEndpoint
{
    public static RouteHandlerBuilder MapActivateDrawGroupEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawGroupId:guid}:activate",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ActivateDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupActivate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateDrawGroup");
    }
}
