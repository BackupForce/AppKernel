using Application.Roles.Create;
using Application.Roles.Delete;
using Application.Roles.Dtos;
using Application.Roles.GetById;
using Application.Roles.List;
using Application.Abstractions.Authorization;
using Application.Roles.Permissions;
using Application.Roles.Update;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;
using Web.Api.Endpoints.Admin.Roles.Endpoints;

namespace Web.Api.Endpoints.Admin.Roles;

internal static class RolesEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/roles")
            .WithTags("Admin.Roles");


        group.MapCreateRoleEndpoint();
        group.MapUpdateRoleEndpoint();
        group.MapDeleteRoleEndpoint();

        group.MapGetRoleEnpoint();
        group.MapGetRolesEndpoint();

        group.MapGetRolePermissionsEndpoint();
        group.MapReplaceRolePermissionsEndpoint();
        group.MapUpdateRolePermissionsEndpoint();
        group.MapRemoveRolePermissionsEndpoint();
    }
}

//public sealed class RolesEndpoints : IEndpoint
//{
//    public void MapEndpoint(IEndpointRouteBuilder app)
//    {
//        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/roles")
//            .WithGroupName("admin-v1")
//            .WithMetadata(new ApiVersion(1, 0))
//            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
//            .WithTags("Roles");


//        group.MapCreateRoleEndpoint();
//        group.MapUpdateRoleEndpoint();
//        group.MapDeleteRoleEndpoint();

//        group.MapGetRoleEnpoint();
//        group.MapGetRolesEndpoint();

//        group.MapGetRolePermissionsEndpoint();
//        group.MapUpdateRolePermissionsEndpoint();
//        group.MapRemoveRolePermissionsEndpoint();
//    }
//}
