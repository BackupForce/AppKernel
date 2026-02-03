using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.Deactivate;
using Application.Gaming.Prizes.GetList;
using Application.Gaming.Prizes.Update;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.SubEndpoints;

internal static class PrizeEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/prizes")
            .WithTags("Gaming.Prizes");

        group.MapGet(
                "/",
                async (ISender sender, CancellationToken ct) =>
                {
                    var query = new GetPrizeListQuery();
                    return await UseCaseInvoker.Send<GetPrizeListQuery, IReadOnlyCollection<PrizeDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<PrizeDto>>(StatusCodes.Status200OK)
            .WithName("GetPrizeList");

        group.MapPost(
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

        group.MapPut(
                "/{prizeId:guid}",
                async (Guid prizeId, UpdatePrizeRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdatePrizeCommand(prizeId, request.Name, request.Description, request.Cost);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdatePrize");

        group.MapPatch(
                "/{prizeId:guid}/activate",
                async (Guid prizeId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ActivatePrizeCommand(prizeId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivatePrize");

        group.MapPatch(
                "/{prizeId:guid}/deactivate",
                async (Guid prizeId, ISender sender, CancellationToken ct) =>
                {
                    var command = new DeactivatePrizeCommand(prizeId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("DeactivatePrize");
    }

}
