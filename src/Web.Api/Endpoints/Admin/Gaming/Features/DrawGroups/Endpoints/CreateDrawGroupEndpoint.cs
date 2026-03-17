using Application.Gaming.DrawGroups.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Requests;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawGroups.Endpoints;

public static class CreateDrawGroupEndpoint
{
    public static RouteHandlerBuilder MapCreateDrawGroupEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (Guid tenantId, CreateDrawGroupRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreateDrawGroupCommand(
                        tenantId,
                        request.GameCode,
                        request.PlayTypeCode,
                        request.Name);
                    return await UseCaseInvoker.Send<CreateDrawGroupCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupCreate.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateDrawGroup");
    }
}
