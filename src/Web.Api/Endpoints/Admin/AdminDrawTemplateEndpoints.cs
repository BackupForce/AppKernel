using Application.Abstractions.Authorization;
using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Activate;
using Application.Gaming.DrawTemplates.Create;
using Application.Gaming.DrawTemplates.Deactivate;
using Application.Gaming.DrawTemplates.GetDetail;
using Application.Gaming.DrawTemplates.GetList;
using Application.Gaming.DrawTemplates.Update;
using Application.Gaming.Dtos;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminDrawTemplateEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin/gaming")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Gaming Templates");

        group.MapPost(
                "/draw-templates",
                async (CreateDrawTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateDrawTemplateCommand command = new CreateDrawTemplateCommand(
                        request.GameCode,
                        request.Name,
                        request.IsActive,
                        request.PlayTypes.Select(playType => new DrawTemplatePlayTypeInput(
                            playType.PlayTypeCode,
                            playType.PrizeTiers.Select(tier => new DrawTemplatePrizeTierInput(
                                tier.Tier,
                                new DrawTemplatePrizeOptionInput(
                                    tier.Option.PrizeId,
                                    tier.Option.Name,
                                    tier.Option.Cost,
                                    tier.Option.RedeemValidDays,
                                    tier.Option.Description))).ToList())).ToList(),
                        request.AllowedTicketTemplateIds);
                    return await UseCaseInvoker.Send<CreateDrawTemplateCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminCreateDrawTemplate");

        group.MapPut(
                "/draw-templates/{templateId:guid}",
                async (Guid templateId, UpdateDrawTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateDrawTemplateCommand command = new UpdateDrawTemplateCommand(
                        templateId,
                        request.Name,
                        request.PlayTypes.Select(playType => new DrawTemplatePlayTypeInput(
                            playType.PlayTypeCode,
                            playType.PrizeTiers.Select(tier => new DrawTemplatePrizeTierInput(
                                tier.Tier,
                                new DrawTemplatePrizeOptionInput(
                                    tier.Option.PrizeId,
                                    tier.Option.Name,
                                    tier.Option.Cost,
                                    tier.Option.RedeemValidDays,
                                    tier.Option.Description))).ToList())).ToList(),
                        request.AllowedTicketTemplateIds);
                    return await UseCaseInvoker.Send(
                        command,
                        sender,
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminUpdateDrawTemplate");

        group.MapPost(
                "/draw-templates/{templateId:guid}/activate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    ActivateDrawTemplateCommand command = new ActivateDrawTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces(StatusCodes.Status200OK)
            .WithName("AdminActivateDrawTemplate");

        group.MapPost(
                "/draw-templates/{templateId:guid}/deactivate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    DeactivateDrawTemplateCommand command = new DeactivateDrawTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces(StatusCodes.Status200OK)
            .WithName("AdminDeactivateDrawTemplate");

        group.MapGet(
                "/draw-templates",
                async ([AsParameters] GetDrawTemplatesRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetDrawTemplatesQuery query = new GetDrawTemplatesQuery(request.GameCode, request.IsActive);
                    return await UseCaseInvoker.Send<GetDrawTemplatesQuery, IReadOnlyCollection<DrawTemplateSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces<IReadOnlyCollection<DrawTemplateSummaryDto>>(StatusCodes.Status200OK)
            .WithName("AdminGetDrawTemplates");

        group.MapGet(
                "/draw-templates/{templateId:guid}",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawTemplateDetailQuery query = new GetDrawTemplateDetailQuery(templateId);
                    return await UseCaseInvoker.Send<GetDrawTemplateDetailQuery, DrawTemplateDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces<DrawTemplateDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetDrawTemplateDetail");
    }
}
