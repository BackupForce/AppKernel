using Application.Abstractions.Data;
using Application.Gaming.DrawGroups.Activate;
using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.Delete;
using Application.Gaming.DrawGroups.Draws.Add;
using Application.Gaming.DrawGroups.Draws.Remove;
using Application.Gaming.DrawGroups.End;
using Application.Gaming.DrawGroups.GetById;
using Application.Gaming.DrawGroups.List;
using Application.Gaming.DrawGroups.Update;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.DrawGroups;

internal static class GamingDrawGroupEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        RouteGroupBuilder drawGroupGroup = group.MapGroup("/draw-groups");
        RouteGroupBuilder campaignGroup = group.MapGroup("/campaigns");

        MapDrawGroupRoutes(drawGroupGroup);
        MapCampaignRoutes(campaignGroup);
    }

    private static void MapDrawGroupRoutes(RouteGroupBuilder drawGroupGroup)
    {
        drawGroupGroup.MapPost(
                "/",
                async (Guid tenantId, CreateDrawGroupRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateDrawGroupCommand command = new CreateDrawGroupCommand(
                        tenantId,
                        request.GameCode,
                        request.PlayTypeCode,
                        request.Name,
                        request.GrantOpenAtUtc,
                        request.GrantCloseAtUtc);
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

        drawGroupGroup.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] ListDrawGroupsRequest request, ISender sender, CancellationToken ct) =>
                {
                    ListDrawGroupsQuery query = new ListDrawGroupsQuery(
                        tenantId,
                        request.Status,
                        request.GameCode,
                        request.Keyword,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<ListDrawGroupsQuery, PagedResult<DrawGroupSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<PagedResult<DrawGroupSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ListDrawGroups");

        drawGroupGroup.MapGet(
                "/{drawGroupId:guid}",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawGroupByIdQuery query = new GetDrawGroupByIdQuery(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send<GetDrawGroupByIdQuery, DrawGroupDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<DrawGroupDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetDrawGroupById");

        drawGroupGroup.MapPut(
                "/{drawGroupId:guid}",
                async (Guid tenantId, Guid drawGroupId, UpdateDrawGroupRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateDrawGroupCommand command = new UpdateDrawGroupCommand(
                        tenantId,
                        drawGroupId,
                        request.Name,
                        request.GrantOpenAtUtc,
                        request.GrantCloseAtUtc);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupUpdate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateDrawGroup");

        drawGroupGroup.MapPost(
                "/{drawGroupId:guid}:activate",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    ActivateDrawGroupCommand command = new ActivateDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupActivate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateDrawGroup");

        drawGroupGroup.MapPost(
                "/{drawGroupId:guid}:end",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    EndDrawGroupCommand command = new EndDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupEnd.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("EndDrawGroup");

        drawGroupGroup.MapPost(
                "/{drawGroupId:guid}/draws",
                async (Guid tenantId, Guid drawGroupId, AddDrawGroupDrawRequest request, ISender sender, CancellationToken ct) =>
                {
                    AddDrawGroupDrawCommand command = new AddDrawGroupDrawCommand(tenantId, drawGroupId, request.DrawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AddDrawGroupDraw");

        drawGroupGroup.MapDelete(
                "/{drawGroupId:guid}/draws/{drawId:guid}",
                async (Guid tenantId, Guid drawGroupId, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    RemoveDrawGroupDrawCommand command = new RemoveDrawGroupDrawCommand(tenantId, drawGroupId, drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoveDrawGroupDraw");

        drawGroupGroup.MapDelete(
                "/{drawGroupId:guid}",
                async (Guid tenantId, Guid drawGroupId, ISender sender, CancellationToken ct) =>
                {
                    DeleteDrawGroupCommand command = new DeleteDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDelete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("DeleteDrawGroup");
    }

    private static void MapCampaignRoutes(RouteGroupBuilder campaignGroup)
    {
        campaignGroup.MapPost(
                "/",
                async (Guid tenantId, CreateDrawGroupRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateDrawGroupCommand command = new CreateDrawGroupCommand(
                        tenantId,
                        request.GameCode,
                        request.PlayTypeCode,
                        request.Name,
                        request.GrantOpenAtUtc,
                        request.GrantCloseAtUtc);
                    return await UseCaseInvoker.Send<CreateDrawGroupCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupCreate.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateCampaign");

        campaignGroup.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] ListDrawGroupsRequest request, ISender sender, CancellationToken ct) =>
                {
                    ListDrawGroupsQuery query = new ListDrawGroupsQuery(
                        tenantId,
                        request.Status,
                        request.GameCode,
                        request.Keyword,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<ListDrawGroupsQuery, PagedResult<DrawGroupSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<PagedResult<DrawGroupSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ListCampaigns");

        campaignGroup.MapGet(
                "/{campaignId:guid}",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawGroupByIdQuery query = new GetDrawGroupByIdQuery(tenantId, campaignId);
                    return await UseCaseInvoker.Send<GetDrawGroupByIdQuery, DrawGroupDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<DrawGroupDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetCampaignById");

        campaignGroup.MapPut(
                "/{campaignId:guid}",
                async (Guid tenantId, Guid campaignId, UpdateDrawGroupRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateDrawGroupCommand command = new UpdateDrawGroupCommand(
                        tenantId,
                        campaignId,
                        request.Name,
                        request.GrantOpenAtUtc,
                        request.GrantCloseAtUtc);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupUpdate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateCampaign");

        campaignGroup.MapPost(
                "/{campaignId:guid}:activate",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    ActivateDrawGroupCommand command = new ActivateDrawGroupCommand(tenantId, campaignId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupActivate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateCampaign");

        campaignGroup.MapPost(
                "/{campaignId:guid}:end",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    EndDrawGroupCommand command = new EndDrawGroupCommand(tenantId, campaignId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupEnd.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("EndCampaign");

        campaignGroup.MapPost(
                "/{campaignId:guid}/draws",
                async (Guid tenantId, Guid campaignId, AddDrawGroupDrawRequest request, ISender sender, CancellationToken ct) =>
                {
                    AddDrawGroupDrawCommand command = new AddDrawGroupDrawCommand(tenantId, campaignId, request.DrawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AddCampaignDraw");

        campaignGroup.MapDelete(
                "/{campaignId:guid}/draws/{drawId:guid}",
                async (Guid tenantId, Guid campaignId, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    RemoveDrawGroupDrawCommand command = new RemoveDrawGroupDrawCommand(tenantId, campaignId, drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoveCampaignDraw");

        campaignGroup.MapDelete(
                "/{campaignId:guid}",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    DeleteDrawGroupCommand command = new DeleteDrawGroupCommand(tenantId, campaignId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDelete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("DeleteCampaign");
    }
}
