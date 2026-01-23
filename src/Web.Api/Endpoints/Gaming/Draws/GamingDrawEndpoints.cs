using Application.Abstractions.Authorization;
using Application.Gaming.Draws.AllowedTicketTemplates.Get;
using Application.Gaming.Draws.AllowedTicketTemplates.Update;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.ManualClose;
using Application.Gaming.Draws.PrizePool;
using Application.Gaming.Draws.PrizePool.Get;
using Application.Gaming.Draws.PrizePool.Update;
using Application.Gaming.Draws.PrizePool.Validate;
using Application.Gaming.Draws.Reopen;
using Application.Gaming.Draws.SellingOptions;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Place;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.Draws;

internal static class GamingDrawEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(
                 "/games/{gameCode}/draws",
                 async (string gameCode, CreateDrawRequest request, ISender sender, CancellationToken ct) =>
                 {
                     string resolvedGameCode = string.IsNullOrWhiteSpace(request.GameCode)
                         ? gameCode
                         : request.GameCode;
                     if (!string.Equals(resolvedGameCode, gameCode, StringComparison.OrdinalIgnoreCase))
                     {
                         return Results.BadRequest("GameCode in path and body must match.");
                     }

                     CreateDrawCommand command = new CreateDrawCommand(
                         resolvedGameCode,
                         request.EnabledPlayTypes,
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
             .RequireAuthorization(Permission.Gaming.DrawCreate.Name)
             .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
             .Produces<Guid>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .WithName("CreateGameDraw");

        group.MapGet(
                "/games/{gameCode}/draws",
                async (string gameCode, [AsParameters] GetDrawsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetOpenDrawsQuery query = new GetOpenDrawsQuery(gameCode, request.Status);
                    return await UseCaseInvoker.Send<GetOpenDrawsQuery, IReadOnlyCollection<DrawSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<IReadOnlyCollection<DrawSummaryDto>>(StatusCodes.Status200OK)
            .WithName("GetGameOpenDraws");

        group.MapGet(
                "/draws/selling/options",
                async ([AsParameters] GetSellingDrawOptionsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetSellingDrawOptionsQuery query = new GetSellingDrawOptionsQuery(
                        request.GameCode,
                        request.PlayTypeCode,
                        request.Take);
                    return await UseCaseInvoker.Send<GetSellingDrawOptionsQuery, IReadOnlyList<DrawSellingOptionDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<IReadOnlyList<DrawSellingOptionDto>>(StatusCodes.Status200OK)
            .WithName("GetSellingDrawOptions");

        group.MapGet(
                "/games/{gameCode}/draws/{drawId:guid}",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawByIdQuery query = new GetDrawByIdQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawByIdQuery, DrawDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<DrawDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetGameDrawById");

        group.MapPost(
                "/games/{gameCode}/draws/{drawId:guid}/tickets",
                async (string gameCode, Guid drawId, PlaceTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    PlaceTicketCommand command = new PlaceTicketCommand(
                        drawId,
                        request.PlayTypeCode,
                        request.TemplateId,
                        request.Lines);
                    return await UseCaseInvoker.Send<PlaceTicketCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("PlaceGameTicket");

        group.MapPost(
                "/games/{gameCode}/draws/{drawId:guid}/execute",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    ExecuteDrawCommand command = new ExecuteDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawExecute.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ExecuteGameDraw");

        group.MapPost(
                "/games/{gameCode}/draws/{drawId:guid}/settle",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    SettleDrawCommand command = new SettleDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SettleGameDraw");

        group.MapPost(
                "/games/{gameCode}/draws/{drawId:guid}/manual-close",
                async (string gameCode, Guid drawId, CloseDrawManuallyRequest request, ISender sender, CancellationToken ct) =>
                {
                    CloseDrawManuallyCommand command = new CloseDrawManuallyCommand(drawId, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawManualClose.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CloseGameDrawManually");

        group.MapPost(
                "/games/{gameCode}/draws/{drawId:guid}/reopen",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    ReopenDrawCommand command = new ReopenDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawReopen.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ReopenGameDraw");

        group.MapGet(
                "/games/{gameCode}/draws/{drawId:guid}/allowed-ticket-templates",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawAllowedTicketTemplatesQuery query = new GetDrawAllowedTicketTemplatesQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawAllowedTicketTemplatesQuery, IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(StatusCodes.Status200OK)
            .WithName("GetGameDrawAllowedTicketTemplates");

        group.MapPut(
                "/games/{gameCode}/draws/{drawId:guid}/allowed-ticket-templates",
                async (string gameCode, Guid drawId, UpdateDrawAllowedTicketTemplatesRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateDrawAllowedTicketTemplatesCommand command = new UpdateDrawAllowedTicketTemplatesCommand(
                        drawId,
                        request.TemplateIds);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawUpdateAllowedTemplates.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateGameDrawAllowedTicketTemplates");

        group.MapGet(
                "/games/{gameCode}/draws/{drawId:guid}/prize-pool",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawPrizePoolQuery query = new GetDrawPrizePoolQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawPrizePoolQuery, DrawPrizePoolDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces<DrawPrizePoolDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("GetGameDrawPrizePool");

        group.MapPut(
                "/games/{gameCode}/draws/{drawId:guid}/prize-pool",
                async (string gameCode, Guid drawId, UpdateDrawPrizePoolRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateDrawPrizePoolCommand command = new UpdateDrawPrizePoolCommand(
                        drawId,
                        request.Items.Select(item => new UpdateDrawPrizePoolItem(
                            item.PlayTypeCode,
                            item.Tier,
                            new PrizeOptionDto(
                                item.Option.PrizeId,
                                item.Option.Name,
                                item.Option.Cost,
                                item.Option.RedeemValidDays,
                                item.Option.Description))).ToList());
                    return await UseCaseInvoker.Send<UpdateDrawPrizePoolCommand, DrawPrizePoolDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces<DrawPrizePoolDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateGameDrawPrizePool");

        group.MapGet(
                "/games/{gameCode}/draws/{drawId:guid}/prize-pool/validation",
                async (string gameCode, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    ValidateDrawPrizePoolQuery query = new ValidateDrawPrizePoolQuery(drawId);
                    return await UseCaseInvoker.Send<ValidateDrawPrizePoolQuery, DrawPrizePoolValidationDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .WithMetadata(new ResourceNodeMetadata("gameCode", "game:"))
            .Produces<DrawPrizePoolValidationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ValidateGameDrawPrizePool");
    }
}
