using Application.Gaming.Dtos;
using Application.Gaming.TicketTemplates.Activate;
using Application.Gaming.TicketTemplates.Create;
using Application.Gaming.TicketTemplates.Deactivate;
using Application.Gaming.TicketTemplates.GetList;
using Application.Gaming.TicketTemplates.Update;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pipelines.Sockets.Unofficial.Arenas;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Deprecated.TicketTemplates.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Deprecated.TicketTemplates;

internal static class TicketTemplateEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/ticket-templates")
            .WithTags("Gaming.TicketTemplates");

        //CRUD
        group.MapCreateTicketTemplateEndpoint();
        group.MapUpdateTicketTemplateEndpoint();
        group.MapGetTicketTemplatesEndpoint();

        //Action
        group.MapDeactivateTicketTemplateEndpoint();
        group.MapActivateTicketTemplateEndpoint();
    }
}
