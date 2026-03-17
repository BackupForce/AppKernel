using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.Update;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class UpdateDrawGroupEndpoint
{
    public static RouteHandlerBuilder MapUpdateDrawGroupEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{drawGroupId:guid}",
                async (Guid tenantId, Guid drawGroupId, UpdateDrawGroupRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateDrawGroupCommand(
                        tenantId,
                        drawGroupId,
                        request.Name);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupUpdate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateDrawGroup");
    }
}
