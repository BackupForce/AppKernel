using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.GetList;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Deprecated.TicketTemplates.Endpoints;

public static class GetTicketTemplatesEndpoint
{
    public static RouteHandlerBuilder MapGetTicketTemplatesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async ([FromQuery] bool activeOnly, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetTicketTemplatesQuery(activeOnly);
                    return await UseCaseInvoker.Send<GetTicketTemplatesQuery, IReadOnlyCollection<TicketTemplateDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<IReadOnlyCollection<TicketTemplateDto>>(StatusCodes.Status200OK)
            .WithName("GetTicketTemplates");
    }
}
