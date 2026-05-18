using Application.Gaming.DrawTemplates;
using Application.Gaming.DrawTemplates.Activate;
using Application.Gaming.DrawTemplates.Create;
using Application.Gaming.DrawTemplates.Update;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawTemplates.Endpoints;

public static class ActivateDrawTemplateEndpoint
{
    public static RouteHandlerBuilder MapActivateDrawTemplateEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{templateId:guid}/activate",
                async (Guid templateId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ActivateDrawTemplateCommand(templateId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawTemplateManage.Name)
            .Produces(StatusCodes.Status200OK)
            .WithName("AdminActivateDrawTemplate");
    }
}
