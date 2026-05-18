using Application.Gaming.Draws.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class CreateDrawEndpoint
{
    public static RouteHandlerBuilder MapCreateDrawEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                 "/",
                 async (CreateDrawRequest request, ISender sender, CancellationToken ct) =>
                 {
                     var command = new CreateDrawCommand(
                         request.TemplateId,
                         request.SalesStartAt,
                         request.SalesCloseAt,
                         request.DrawAt,
                         request.RedeemValidDays);
                     return await UseCaseInvoker.Send<CreateDrawCommand, Guid>(
                         command,
                         sender,
                         value => Results.Ok(value),
                         ct);
                 })
             .RequireAuthorization(Permission.GamingDraw.Create.Name)
             .Produces<Guid>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .WithName("CreateGameDraw");
    }
}
