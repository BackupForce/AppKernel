using Application.Abstractions.Data;
using Application.Gaming.DrawGroups.Activate;
using Application.Gaming.DrawGroups.Create;
using Application.Gaming.DrawGroups.Delete;
using Application.Gaming.DrawGroups.Draws.Add;
using Application.Gaming.DrawGroups.Draws.Remove;
using Application.Gaming.DrawGroups.End;
using Application.Gaming.DrawGroups.GetById;
using Application.Gaming.DrawGroups.List;
using Application.Gaming.DrawGroups.RemoteSearch;
using Application.Gaming.DrawGroups.Update;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.SubEndpoints;

internal static class DrawGroupEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder drawGroupGroup = parent.MapGroup("/drawgroups")
            .WithTags("Gaming.DrawGroups");



        MapDrawGroupRoutes(drawGroupGroup);
    }

    private static void MapDrawGroupRoutes(RouteGroupBuilder drawGroupGroup)
    {
        drawGroupGroup.MapPost(
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

        drawGroupGroup.MapGet(
                "/remote-search",
                async (Guid tenantId, [AsParameters] RemoteSearchDrawGroupsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new RemoteSearchDrawGroupsQuery(
                        tenantId,
                        request.Q,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<RemoteSearchDrawGroupsQuery, PagedResult<DrawGroupRemoteSearchDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupRead.Name)
            .Produces<PagedResult<DrawGroupRemoteSearchDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoteSearchDrawGroups");

        drawGroupGroup.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] ListDrawGroupsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new ListDrawGroupsQuery(
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
                    var query = new GetDrawGroupByIdQuery(tenantId, drawGroupId);
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
                    var command = new UpdateDrawGroupCommand(
                        tenantId,
                        drawGroupId,
                        request.Name);
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
                    var command = new ActivateDrawGroupCommand(tenantId, drawGroupId);
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
                    var command = new EndDrawGroupCommand(tenantId, drawGroupId);
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
                    var command = new AddDrawGroupDrawCommand(tenantId, drawGroupId, request.DrawId);
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
                    var command = new RemoveDrawGroupDrawCommand(tenantId, drawGroupId, drawId);
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
                    var command = new DeleteDrawGroupCommand(tenantId, drawGroupId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawGroupDelete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("DeleteDrawGroup");
    }
}
