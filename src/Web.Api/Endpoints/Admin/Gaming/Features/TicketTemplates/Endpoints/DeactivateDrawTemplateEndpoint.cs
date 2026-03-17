using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Activate;
using Application.Gaming.DrawTemplates.Create;
using Application.Gaming.DrawTemplates.Deactivate;
using Application.Gaming.DrawTemplates.Update;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

public static class DeactivateDrawTemplateEndpoint
{
    public static RouteHandlerBuilder MapDeactivateDrawTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/draw-templates/{templateId:guid}/deactivate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    var command = new DeactivateDrawTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces(StatusCodes.Status200OK)
            .WithName("AdminDeactivateDrawTemplate");
    }
}
