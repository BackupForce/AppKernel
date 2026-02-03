using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
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
using Application.Gaming.Draws.RemoteSearch;
using Application.Gaming.Draws.Reopen;
using Application.Gaming.Draws.SellingOptions;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Place;
using Domain.Security;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.SubEndpoints;

internal static class DrawEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent
            .MapGroup("/draws")
            .WithTags("Gaming.Draws");

        group.MapPost(
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
             .RequireAuthorization(Permission.Gaming.DrawCreate.Name)
             .Produces<Guid>(StatusCodes.Status200OK)
             .ProducesProblem(StatusCodes.Status400BadRequest)
             .WithName("CreateGameDraw");

        group.MapGet(
                "/remote-search",
                async (Guid tenantId, [AsParameters] RemoteSearchDrawsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new RemoteSearchDrawsQuery(
                        tenantId,
                        request.Q,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<RemoteSearchDrawsQuery, PagedResult<DrawRemoteSearchDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<PagedResult<DrawRemoteSearchDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoteSearchDraws");

        group.MapGet(
                "/",
                async ([AsParameters] GetDrawsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetOpenDrawsQuery(request.Status);
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
                "/selling/options",
                async ([AsParameters] GetSellingDrawOptionsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetSellingDrawOptionsQuery(
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
            .WithSummary("可售票期數下拉選項")
            .WithDescription("可售票期數下拉選項")
            .WithName("GetSellingDrawOptions");

        group.MapGet(
                "/{drawId:guid}",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawByIdQuery(drawId);
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
                "/{drawId:guid}/execute",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ExecuteDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawExecute.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ExecuteGameDraw");

        group.MapPost(
                "/{drawId:guid}/settle",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new SettleDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SettleGameDraw");

        group.MapPost(
                "/{drawId:guid}/manual-close",
                async (Guid drawId, CloseDrawManuallyRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CloseDrawManuallyCommand(drawId, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawManualClose.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CloseGameDrawManually");

        group.MapPost(
                "/{drawId:guid}/reopen",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ReopenDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawReopen.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ReopenGameDraw");

        group.MapGet(
                "/{drawId:guid}/allowed-ticket-templates",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawAllowedTicketTemplatesQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawAllowedTicketTemplatesQuery, IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(StatusCodes.Status200OK)
            .WithName("GetGameDrawAllowedTicketTemplates");

        group.MapPut(
                "/{drawId:guid}/allowed-ticket-templates",
                async (Guid drawId, UpdateDrawAllowedTicketTemplatesRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateDrawAllowedTicketTemplatesCommand(
                        drawId,
                        request.TemplateIds);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawUpdateAllowedTemplates.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateGameDrawAllowedTicketTemplates");

        group.MapGet(
                "/{drawId:guid}/prize-pool",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawPrizePoolQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawPrizePoolQuery, DrawPrizePoolDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces<DrawPrizePoolDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("GetGameDrawPrizePool");

        group.MapPut(
                "/{drawId:guid}/prize-pool",
                async (Guid drawId, UpdateDrawPrizePoolRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateDrawPrizePoolCommand(
                        drawId,
                        request.Items.Select(item => new UpdateDrawPrizePoolItem(
                            item.PlayTypeCode,
                            item.Tier,
                            new PrizeOptionDto(
                                item.Option.PrizeId,
                                item.Option.Name,
                                item.Option.Cost,
                                item.Option.PayoutAmount,
                                item.Option.RedeemValidDays,
                                item.Option.Description))).ToList());
                    return await UseCaseInvoker.Send<UpdateDrawPrizePoolCommand, DrawPrizePoolDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces<DrawPrizePoolDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateGameDrawPrizePool");

        group.MapGet(
                "/{drawId:guid}/prize-pool/validation",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    var query = new ValidateDrawPrizePoolQuery(drawId);
                    return await UseCaseInvoker.Send<ValidateDrawPrizePoolQuery, DrawPrizePoolValidationDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces<DrawPrizePoolValidationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ValidateGameDrawPrizePool");
    }
}
