using Web.Api.Endpoints.Admin.Users.Endpoints;

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

        //Role management
        group.MapAssignRoleToUserEndpoint();
        group.MapRemoveUserRoleEndpoint();
    }
}
