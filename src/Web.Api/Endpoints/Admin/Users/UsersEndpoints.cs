
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


        group.MapPost(
                "/",
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

        group.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] GetTenantUsersRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetTenantUsersQuery query = new GetTenantUsersQuery(tenantId, request.Page, request.PageSize);
                    return await UseCaseInvoker.Send<GetTenantUsersQuery, PagedResult<TenantUserListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.View.Name)
            .Produces<PagedResult<TenantUserListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("GetTenantUsers");

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


        group.MapDelete(
                "/{userId:guid}/roles/{roleName}",
                async (Guid userId, string roleName, ISender sender, CancellationToken ct) =>
                {
                    RemoveUserRoleCommand command = new RemoveUserRoleCommand(userId, roleName);
                    Result<RemoveUserRoleResultDto> result = await sender.Send(command, ct);
                    return result.Match(
                        _ => Results.NoContent(),
                        failure => CustomResults.Problem(failure));
                })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("RemoveUserRole");
    }
}
