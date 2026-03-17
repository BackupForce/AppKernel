
using Application.Abstractions.Data;
using Application.Users.AssignRole;
using Application.Users.Create;
using Application.Users.GetById;
using Application.Users.GetTenantUsers;
using Application.Users.RemoveRole;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Users.Endpoints;
using Web.Api.Endpoints.Users.Requests;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin.Users;

internal static class UsersEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/users")
            .WithTags("Admin.Users");


        group.MapCreateUserEndpoint();
        group.MapGetUserEndpoint();
        group.MapGetUsersEndpoint();
        group.MapResetUserPasswordEndpoint();
        group.MapDisableUserEndpoint();
        group.MapEnableUserEndpoint();

        //Role management
        group.MapAssignRoleToUserEndpoint();
        group.MapRemoveUserRoleEndpoint();
    }
}
