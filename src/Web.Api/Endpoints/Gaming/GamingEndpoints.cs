using Application.Abstractions.Authorization;
using Application.Gaming.Awards.GetMy;
using Application.Gaming.Awards.Redeem;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.Execute;
using Application.Gaming.Draws.GetById;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Draws.ManualClose;
using Application.Gaming.Draws.Reopen;
using Application.Gaming.Draws.Settle;
using Application.Gaming.Draws.AllowedTicketTemplates.Get;
using Application.Gaming.Draws.AllowedTicketTemplates.Update;
using Application.Gaming.Draws.PrizeMappings.Get;
using Application.Gaming.Draws.PrizeMappings.Update;
using Application.Gaming.PrizeRules.Activate;
using Application.Gaming.PrizeRules.Create;
using Application.Gaming.PrizeRules.Deactivate;
using Application.Gaming.PrizeRules.GetList;
using Application.Gaming.PrizeRules.Update;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.Deactivate;
using Application.Gaming.Prizes.GetList;
using Application.Gaming.Prizes.Update;
using Application.Gaming.Tickets.GetMy;
using Application.Gaming.Tickets.Place;
using Application.Gaming.TicketTemplates.Activate;
using Application.Gaming.TicketTemplates.Create;
using Application.Gaming.TicketTemplates.Deactivate;
using Application.Gaming.TicketTemplates.GetList;
using Application.Gaming.TicketTemplates.Update;
using Application.Gaming.Dtos;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming;

/// <summary>
/// Gaming 模組 API 路由，負責授權與 request/response 轉換。
/// </summary>
/// <remarks>
/// Web.Api 僅負責路由與授權，不承載業務邏輯。
/// </remarks>
public sealed class GamingEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // 依租戶隔離路由，權限由 AuthorizationPolicy 控制。
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/gaming")
            .WithGroupName("tenant-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Gaming");

        MapDrawEndpoints(group);
        MapPrizeEndpoints(group);
        MapPrizeRuleEndpoints(group);
        MapTicketTemplateEndpoints(group);
        MapMemberEndpoints(group);
        MapRedeemEndpoints(group);
    }

    private static void MapDrawEndpoints(RouteGroupBuilder group)
    {
        group.MapPost(
                "/lottery539/draws",
                async (CreateDrawRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateDrawCommand command = new CreateDrawCommand(
                        request.GameCode,
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
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateLottery539Draw");

        group.MapGet(
                "/lottery539/draws",
                async ([AsParameters] GetDrawsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetOpenDrawsQuery query = new GetOpenDrawsQuery(request.Status);
                    return await UseCaseInvoker.Send<GetOpenDrawsQuery, IReadOnlyCollection<DrawSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<IReadOnlyCollection<DrawSummaryDto>>(StatusCodes.Status200OK)
            .WithName("GetLottery539OpenDraws");

        group.MapGet(
                "/lottery539/draws/{drawId:guid}",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
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
            .WithName("GetLottery539DrawById");

        group.MapPost(
                "/lottery539/draws/{drawId:guid}/tickets",
                async (Guid drawId, PlaceTicketRequest request, ISender sender, CancellationToken ct) =>
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
            .WithName("PlaceLottery539Ticket");

        group.MapPost(
                "/lottery539/draws/{drawId:guid}/execute",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    ExecuteDrawCommand command = new ExecuteDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ExecuteLottery539Draw");

        group.MapPost(
                "/lottery539/draws/{drawId:guid}/settle",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    SettleDrawCommand command = new SettleDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SettleLottery539Draw");

        group.MapPost(
                "/lottery539/draws/{drawId:guid}/manual-close",
                async (Guid drawId, CloseDrawManuallyRequest request, ISender sender, CancellationToken ct) =>
                {
                    CloseDrawManuallyCommand command = new CloseDrawManuallyCommand(drawId, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CloseLottery539DrawManually");

        group.MapPost(
                "/lottery539/draws/{drawId:guid}/reopen",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    ReopenDrawCommand command = new ReopenDrawCommand(drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ReopenLottery539Draw");

        group.MapGet(
                "/lottery539/draws/{drawId:guid}/allowed-ticket-templates",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawAllowedTicketTemplatesQuery query = new GetDrawAllowedTicketTemplatesQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawAllowedTicketTemplatesQuery, IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(StatusCodes.Status200OK)
            .WithName("GetDrawAllowedTicketTemplates");

        group.MapPut(
                "/lottery539/draws/{drawId:guid}/allowed-ticket-templates",
                async (Guid drawId, UpdateDrawAllowedTicketTemplatesRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateDrawAllowedTicketTemplatesCommand command = new UpdateDrawAllowedTicketTemplatesCommand(
                        drawId,
                        request.TemplateIds);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateDrawAllowedTicketTemplates");

        group.MapGet(
                "/lottery539/draws/{drawId:guid}/prize-mappings",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawPrizeMappingsQuery query = new GetDrawPrizeMappingsQuery(drawId);
                    return await UseCaseInvoker.Send<GetDrawPrizeMappingsQuery, IReadOnlyCollection<DrawPrizeMappingDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<DrawPrizeMappingDto>>(StatusCodes.Status200OK)
            .WithName("GetDrawPrizeMappings");

        group.MapPut(
                "/lottery539/draws/{drawId:guid}/prize-mappings",
                async (Guid drawId, UpdateDrawPrizeMappingsRequest request, ISender sender, CancellationToken ct) =>
                {
                    List<DrawPrizeMappingInput> mappings = new List<DrawPrizeMappingInput>();
                    foreach (DrawPrizeMappingItemRequest item in request.Mappings)
                    {
                        mappings.Add(new DrawPrizeMappingInput(item.MatchCount, item.PrizeIds));
                    }

                    UpdateDrawPrizeMappingsCommand command = new UpdateDrawPrizeMappingsCommand(drawId, mappings);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateDrawPrizeMappings");
    }

    private static void MapMemberEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(
                "/lottery539/members/me/tickets",
                async ([AsParameters] GetMyTicketsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMyTicketsQuery query = new GetMyTicketsQuery(request.From, request.To);
                    return await UseCaseInvoker.Send<GetMyTicketsQuery, IReadOnlyCollection<TicketSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IReadOnlyCollection<TicketSummaryDto>>(StatusCodes.Status200OK)
            .WithName("GetMyLottery539Tickets");

        group.MapGet(
                "/lottery539/members/me/awards",
                async ([AsParameters] GetMyAwardsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMyAwardsQuery query = new GetMyAwardsQuery(request.Status);
                    return await UseCaseInvoker.Send<GetMyAwardsQuery, IReadOnlyCollection<PrizeAwardDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IReadOnlyCollection<PrizeAwardDto>>(StatusCodes.Status200OK)
            .WithName("GetMyPrizeAwards");
    }

    private static void MapRedeemEndpoints(RouteGroupBuilder group)
    {
        group.MapPost(
                "/prizes/awards/{awardId:guid}/redeem",
                async (Guid awardId, RedeemPrizeAwardRequest request, ISender sender, CancellationToken ct) =>
                {
                    RedeemPrizeAwardCommand command = new RedeemPrizeAwardCommand(awardId, request.Note);
                    return await UseCaseInvoker.Send<RedeemPrizeAwardCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RedeemPrizeAward");
    }

    private static void MapPrizeEndpoints(RouteGroupBuilder group)
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

    private static void MapTicketTemplateEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(
                "/ticket-templates",
                async ([FromQuery] bool activeOnly, ISender sender, CancellationToken ct) =>
                {
                    GetTicketTemplatesQuery query = new GetTicketTemplatesQuery(activeOnly);
                    return await UseCaseInvoker.Send<GetTicketTemplatesQuery, IReadOnlyCollection<TicketTemplateDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<TicketTemplateDto>>(StatusCodes.Status200OK)
            .WithName("GetTicketTemplates");

        group.MapPost(
                "/ticket-templates",
                async (CreateTicketTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateTicketTemplateCommand command = new CreateTicketTemplateCommand(
                        request.Code,
                        request.Name,
                        request.Type,
                        request.Price,
                        request.ValidFrom,
                        request.ValidTo,
                        request.MaxLinesPerTicket);
                    return await UseCaseInvoker.Send<CreateTicketTemplateCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateTicketTemplate");

        group.MapPut(
                "/ticket-templates/{templateId:guid}",
                async (Guid templateId, UpdateTicketTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateTicketTemplateCommand command = new UpdateTicketTemplateCommand(
                        templateId,
                        request.Code,
                        request.Name,
                        request.Type,
                        request.Price,
                        request.ValidFrom,
                        request.ValidTo,
                        request.MaxLinesPerTicket);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateTicketTemplate");

        group.MapPatch(
                "/ticket-templates/{templateId:guid}/activate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    ActivateTicketTemplateCommand command = new ActivateTicketTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivateTicketTemplate");

        group.MapPatch(
                "/ticket-templates/{templateId:guid}/deactivate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    DeactivateTicketTemplateCommand command = new DeactivateTicketTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("DeactivateTicketTemplate");
    }

    private static void MapPrizeRuleEndpoints(RouteGroupBuilder group)
    {
        group.MapGet(
                "/lottery539/prize-rules",
                async (ISender sender, CancellationToken ct) =>
                {
                    GetPrizeRuleListQuery query = new GetPrizeRuleListQuery();
                    return await UseCaseInvoker.Send<GetPrizeRuleListQuery, IReadOnlyCollection<PrizeRuleDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<PrizeRuleDto>>(StatusCodes.Status200OK)
            .WithName("GetPrizeRuleList");

        group.MapPost(
                "/lottery539/prize-rules",
                async (CreatePrizeRuleRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreatePrizeRuleCommand command = new CreatePrizeRuleCommand(
                        request.MatchCount,
                        request.PrizeId,
                        request.EffectiveFrom,
                        request.EffectiveTo,
                        request.RedeemValidDays);
                    return await UseCaseInvoker.Send<CreatePrizeRuleCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreatePrizeRule");

        group.MapPut(
                "/lottery539/prize-rules/{ruleId:guid}",
                async (Guid ruleId, UpdatePrizeRuleRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdatePrizeRuleCommand command = new UpdatePrizeRuleCommand(
                        ruleId,
                        request.MatchCount,
                        request.PrizeId,
                        request.EffectiveFrom,
                        request.EffectiveTo,
                        request.RedeemValidDays);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdatePrizeRule");

        group.MapPatch(
                "/lottery539/prize-rules/{ruleId:guid}/activate",
                async (Guid ruleId, ISender sender, CancellationToken ct) =>
                {
                    ActivatePrizeRuleCommand command = new ActivatePrizeRuleCommand(ruleId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivatePrizeRule");

        group.MapPatch(
                "/lottery539/prize-rules/{ruleId:guid}/deactivate",
                async (Guid ruleId, ISender sender, CancellationToken ct) =>
                {
                    DeactivatePrizeRuleCommand command = new DeactivatePrizeRuleCommand(ruleId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("DeactivatePrizeRule");
    }
}
