using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Create;
using Application.Gaming.DrawTemplates.GetList;
using Application.Gaming.DrawTemplates.Update;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class GetDrawTemplatesEndpoint
{
    public static RouteHandlerBuilder MapGetDrawTemplatesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/draw-templates",
                async ([AsParameters] GetDrawTemplatesRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawTemplatesQuery(request.GameCode, request.IsActive);
                    return await UseCaseInvoker.Send<GetDrawTemplatesQuery, IReadOnlyCollection<DrawTemplateSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces<IReadOnlyCollection<DrawTemplateSummaryDto>>(StatusCodes.Status200OK)
            .WithName("AdminGetDrawTemplates");
    }
}
