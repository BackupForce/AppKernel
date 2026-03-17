using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Create;
using Application.Gaming.DrawTemplates.Update;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class UpdateDrawTemplateEndpoint
{
    public static RouteHandlerBuilder MapUpdateDrawTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/draw-templates/{templateId:guid}",
                async (Guid templateId, UpdateDrawTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateDrawTemplateCommand(
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
                                    tier.Option.PayoutAmount,
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
    }
}
