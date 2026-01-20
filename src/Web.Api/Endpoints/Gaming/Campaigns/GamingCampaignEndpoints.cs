using Application.Abstractions.Data;
using Application.Gaming.Campaigns.Activate;
using Application.Gaming.Campaigns.Create;
using Application.Gaming.Campaigns.Delete;
using Application.Gaming.Campaigns.Draws.Add;
using Application.Gaming.Campaigns.Draws.Remove;
using Application.Gaming.Campaigns.End;
using Application.Gaming.Campaigns.GetById;
using Application.Gaming.Campaigns.List;
using Application.Gaming.Campaigns.Update;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Gaming.Requests;

namespace Web.Api.Endpoints.Gaming.Campaigns;

internal static class GamingCampaignEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        RouteGroupBuilder campaignGroup = group.MapGroup("/campaigns");

        campaignGroup.MapPost(
                "/",
                async (Guid tenantId, CreateCampaignRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateCampaignCommand command = new CreateCampaignCommand(
                        tenantId,
                        request.GameCode,
                        request.PlayTypeCode,
                        request.Name,
                        request.GrantOpenAtUtc,
                        request.GrantCloseAtUtc);
                    return await UseCaseInvoker.Send<CreateCampaignCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignCreate.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateCampaign");

        campaignGroup.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] ListCampaignsRequest request, ISender sender, CancellationToken ct) =>
                {
                    ListCampaignsQuery query = new ListCampaignsQuery(
                        tenantId,
                        request.Status,
                        request.GameCode,
                        request.Keyword,
                        request.Page,
                        request.PageSize);
                    return await UseCaseInvoker.Send<ListCampaignsQuery, PagedResult<CampaignSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignRead.Name)
            .Produces<PagedResult<CampaignSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ListCampaigns");

        campaignGroup.MapGet(
                "/{campaignId:guid}",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    GetCampaignByIdQuery query = new GetCampaignByIdQuery(tenantId, campaignId);
                    return await UseCaseInvoker.Send<GetCampaignByIdQuery, CampaignDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignRead.Name)
            .Produces<CampaignDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetCampaignById");

        campaignGroup.MapPut(
                "/{campaignId:guid}",
                async (Guid tenantId, Guid campaignId, UpdateCampaignRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateCampaignCommand command = new UpdateCampaignCommand(
                        tenantId,
                        campaignId,
                        request.Name,
                        request.GrantOpenAtUtc,
                        request.GrantCloseAtUtc);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignUpdate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateCampaign");

        campaignGroup.MapPost(
                "/{campaignId:guid}:activate",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    ActivateCampaignCommand command = new ActivateCampaignCommand(tenantId, campaignId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignActivate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateCampaign");

        campaignGroup.MapPost(
                "/{campaignId:guid}:end",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    EndCampaignCommand command = new EndCampaignCommand(tenantId, campaignId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignEnd.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("EndCampaign");

        campaignGroup.MapPost(
                "/{campaignId:guid}/draws",
                async (Guid tenantId, Guid campaignId, AddCampaignDrawRequest request, ISender sender, CancellationToken ct) =>
                {
                    AddCampaignDrawCommand command = new AddCampaignDrawCommand(tenantId, campaignId, request.DrawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AddCampaignDraw");

        campaignGroup.MapDelete(
                "/{campaignId:guid}/draws/{drawId:guid}",
                async (Guid tenantId, Guid campaignId, Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    RemoveCampaignDrawCommand command = new RemoveCampaignDrawCommand(tenantId, campaignId, drawId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignDrawManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("RemoveCampaignDraw");

        campaignGroup.MapDelete(
                "/{campaignId:guid}",
                async (Guid tenantId, Guid campaignId, ISender sender, CancellationToken ct) =>
                {
                    DeleteCampaignCommand command = new DeleteCampaignCommand(tenantId, campaignId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.CampaignDelete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("DeleteCampaign");
    }
}
