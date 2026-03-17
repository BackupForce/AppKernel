using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class CreateDrawTemplateEndpoint
{
    public static RouteHandlerBuilder MapCreateDrawTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/draw-templates",
                async (CreateDrawTemplateRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreateDrawTemplateCommand(
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
                                    tier.Option.PayoutAmount,
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
    }
}
