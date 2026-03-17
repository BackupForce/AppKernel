using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Create;
using Application.Gaming.DrawTemplates.GetDetail;
using Application.Gaming.DrawTemplates.Update;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class GetDrawTemplateDetailEndpoint
{
    public static RouteHandlerBuilder MapGetDrawTemplateDetailEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/draw-templates/{templateId:guid}",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetDrawTemplateDetailQuery(templateId);
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
