using Application.Users.AssignRole;
using Application.Users.Create;
using Application.Users.GetById;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Users.Handler;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public class UsersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/users")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(2, 0))
            .RequireAuthorization()
            .WithTags("Users");

        group.MapGet("/{id:guid}", GetUserById.Handler)
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithName("GetUserById");

        // <summary>
        // Creates a new user with the provided details.
        // 想一下到底要不要用Handler
        // </summary>
        group.MapPost("/",
            UseCaseInvoker.FromRoute<CreateUserCommand, CreateUserRequest, Guid>(
                req => new CreateUserCommand(
                    req.Email,
                    req.Name,
                    req.Password,
                    req.HasPublicProfile)))
        .Produces<Guid>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithName("CreateUser");

        group.MapPost(
                "/{userId:guid}/roles/{roleId:int}",
                UseCaseInvoker.FromRoute<AssignRoleToUserCommand, Guid, int, AssignRoleToUserResultDto>(
                    (userId, roleId) => new AssignRoleToUserCommand(userId, roleId)))
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces<AssignRoleToUserResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("AssignRoleToUser");
    }
}
