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
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Endpoints;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketTemplates;

public sealed class DrawTemplateEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin/gaming")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Gaming Templates");

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
