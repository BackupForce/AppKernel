using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.End;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class EndDrawGroupEndpoint
{
    public static RouteHandlerBuilder MapEndDrawGroupEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{drawGroupId:guid}:end",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    var command = new EndDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupEnd.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("EndDrawGroup");
    }
}
