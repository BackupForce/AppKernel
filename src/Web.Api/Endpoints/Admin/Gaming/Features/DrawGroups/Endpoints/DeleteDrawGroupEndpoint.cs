using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.Delete;
using Application.Gaming.DrawGroups.End;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class DeleteDrawGroupEndpoint
{
    public static RouteHandlerBuilder MapDeleteDrawGroupEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapDelete(
                "/{drawGroupId:guid}",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    var command = new DeleteDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDelete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("DeleteDrawGroup");
    }
}
