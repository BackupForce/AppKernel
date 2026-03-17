using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.Draws.Add;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class AddDrawGroupDrawEndpoint
{
    public static RouteHandlerBuilder MapAddDrawGroupDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawGroupId:guid}/draws",
                async (Guid tenantId, Guid drawGroupId, AddDrawGroupDrawRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new AddDrawGroupDrawCommand(tenantId, drawGroupId, request.DrawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AddDrawGroupDraw");
    }
}
