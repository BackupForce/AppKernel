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
using Web.Api.Endpoints.Roles.Requests;

namespace Web.Api.Endpoints.Roles;

public sealed class RolesEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/roles")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization()
            .WithTags("Roles");

        group.MapPost(
                "/",
                (CreateRoleRequest request, ISender sender, CancellationToken ct) =>
                    UseCaseInvoker.Handle<CreateRoleCommand, int>(
                        new CreateRoleCommand(request.Name),
                        sender,
                        ct))
            .RequireAuthorization(Permission.Roles.Create.Name)
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateRole");

        group.MapPut(
                "/{id:int}",
                UseCaseInvoker.FromRoute<UpdateRoleCommand, int, UpdateRoleRequest>(
                    (id, request) => new UpdateRoleCommand(id, request.Name)))
            .RequireAuthorization(Permission.Roles.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateRole");

        group.MapDelete(
                "/{id:int}",
                UseCaseInvoker.FromRoute<DeleteRoleCommand, int>(
                    id => new DeleteRoleCommand(id)))
            .RequireAuthorization(Permission.Roles.Delete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteRole");

        group.MapGet(
                "/{id:int}",
                UseCaseInvoker.FromRoute<GetRoleByIdQuery, int, RoleDetailDto>(
                    id => new GetRoleByIdQuery(id)))
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<RoleDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetRoleById");

        group.MapGet(
                "/",
                (ISender sender, CancellationToken ct) =>
                    UseCaseInvoker.Handle<ListRolesQuery, IReadOnlyList<RoleListItemDto>>(
                        new ListRolesQuery(),
                        sender,
                        ct))
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<IReadOnlyList<RoleListItemDto>>(StatusCodes.Status200OK)
            .WithName("ListRoles");

        group.MapGet(
                "/{id:int}/permissions",
                UseCaseInvoker.FromRoute<GetRolePermissionsQuery, int, IReadOnlyList<string>>(
                    id => new GetRolePermissionsQuery(id)))
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetRolePermissions");

        group.MapPost(
                "/{id:int}/permissions",
                UseCaseInvoker.FromRoute<AddRolePermissionsCommand, int, UpdateRolePermissionsRequest>(
                    (id, request) => new AddRolePermissionsCommand(id, request.PermissionCodes)))
            .RequireAuthorization(Permission.Roles.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AddRolePermissions");

        group.MapPost(
            "/{id:int}/permissions/remove",
            UseCaseInvoker.FromRoute<RemoveRolePermissionsCommand, int, UpdateRolePermissionsRequest>(
                (id, request) => new RemoveRolePermissionsCommand(id, request.PermissionCodes)))
        .RequireAuthorization(Permission.Roles.Update.Name)
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithName("RemoveRolePermissions");
    }
}
