using Application.Abstractions.Authorization;
using Application.Users.AssignRole;
using Application.Users.Create;
using Application.Users.GetById;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Users;

public class UsersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/users")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(2, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Users");

        // 改用顯式 handler 參數宣告，避免 Minimal API 推斷參數造成綁定錯誤。
        group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                GetUserByIdQuery request = new GetUserByIdQuery(id);
                return await UseCaseInvoker.Send<GetUserByIdQuery, UserResponse>(
                    request,
                    sender,
                    value => Results.Ok(value),
                    ct);
            })
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithName("GetUserById");

        // <summary>
        // Creates a new user with the provided details.
        // 想一下到底要不要用Handler
        // </summary>
        group.MapPost("/",
            async (CreateUserRequest request, ISender sender, CancellationToken ct) =>
            {
                CreateUserCommand command = new CreateUserCommand(
                    request.Email,
                    request.Name,
                    request.Password,
                    request.HasPublicProfile,
                    request.UserType,
                    request.TenantId);
                return await UseCaseInvoker.Send<CreateUserCommand, Guid>(
                    command,
                    sender,
                    value => Results.Ok(value),
                    ct);
            })
        .Produces<Guid>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithName("CreateUser");

        group.MapPost(
                "/tenant",
                async (CreateTenantUserRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateUserCommand command = new CreateUserCommand(
                        request.Email,
                        request.Name,
                        request.Password,
                        request.HasPublicProfile,
                        null,
                        null);
                    return await UseCaseInvoker.Send<CreateUserCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.Create.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateTenantUser");

        group.MapPost(
                "/{userId:guid}/roles/{roleId:int}",
                async (Guid userId, int roleId, ISender sender, CancellationToken ct) =>
                {
                    AssignRoleToUserCommand command = new AssignRoleToUserCommand(userId, roleId);
                    return await UseCaseInvoker.Send<AssignRoleToUserCommand, AssignRoleToUserResultDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces<AssignRoleToUserResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("AssignRoleToUser");
    }
}
