using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.GetList;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Endpoints;

public static class CreatePrizeEndpoint
{
    public static RouteHandlerBuilder MapCreatePrizeEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (CreatePrizeRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreatePrizeCommand(request.Name, request.Description, request.Cost);
                    return await UseCaseInvoker.Send<CreatePrizeCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create prize")
            .WithDescription("Create a new prize for the game")
            .WithName("CreatePrize");
    }
}
