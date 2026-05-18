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
using Web.Api.Endpoints.Admin.Gaming.Features.DrawTemplates.Endpoints;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.DrawTemplates;

internal static class DrawTemplateEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/draw-templates")
            .WithTags("Gaming.DrawTemplates");

        //CQRS
        group.MapCreateDrawTemplateEndpoint();
        group.MapUpdateDrawTemplateEndpoint();
        group.MapGetDrawTemplateDetailEndpoint();
        group.MapGetDrawTemplatesEndpoint();

        //Action
        group.MapActivateDrawTemplateEndpoint();
        group.MapDeactivateDrawTemplateEndpoint();
    }
}
