using Application.Gaming.Draws.AllowedTicketTemplates.Get;
using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.GetOpen;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetDrawAllowedTicketTemplatesEndpoint
{
    public static RouteHandlerBuilder MapGetDrawAllowedTicketTemplatesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
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
    }
}
