using Application.Abstractions.Authorization;
using Application.Roles.Create;
using Application.Roles.Delete;
using Application.Roles.Dtos;
using Application.Roles.GetById;
using Application.Roles.List;
using Application.Roles.Permissions;
using Application.Roles.Update;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Me.Endpoints;
using Web.Api.Endpoints.Admin.Roles.Endpoints;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Me;

internal static class MeEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/me")
            .WithTags("Admin.Me");

        group.MapGetMyPermissionsEndpoint();
    }
}
