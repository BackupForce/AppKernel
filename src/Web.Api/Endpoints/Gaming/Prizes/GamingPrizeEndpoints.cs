using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.Deactivate;
using Application.Gaming.Prizes.GetList;
using Application.Gaming.Prizes.Update;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.Prizes;

internal static class GamingPrizeEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(
                "/prizes",
                async (ISender sender, CancellationToken ct) =>
                {
                    GetPrizeListQuery query = new GetPrizeListQuery();
                    return await UseCaseInvoker.Send<GetPrizeListQuery, IReadOnlyCollection<PrizeDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<PrizeDto>>(StatusCodes.Status200OK)
            .WithName("GetPrizeList");

        group.MapPost(
                "/prizes",
                async (CreatePrizeRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreatePrizeCommand command = new CreatePrizeCommand(request.Name, request.Description, request.Cost);
                    return await UseCaseInvoker.Send<CreatePrizeCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreatePrize");

        group.MapPut(
                "/prizes/{prizeId:guid}",
                async (Guid prizeId, UpdatePrizeRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdatePrizeCommand command = new UpdatePrizeCommand(prizeId, request.Name, request.Description, request.Cost);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdatePrize");

        group.MapPatch(
                "/prizes/{prizeId:guid}/activate",
                async (Guid prizeId, ISender sender, CancellationToken ct) =>
                {
                    ActivatePrizeCommand command = new ActivatePrizeCommand(prizeId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivatePrize");

        group.MapPatch(
                "/prizes/{prizeId:guid}/deactivate",
                async (Guid prizeId, ISender sender, CancellationToken ct) =>
                {
                    DeactivatePrizeCommand command = new DeactivatePrizeCommand(prizeId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("DeactivatePrize");
    }

}
