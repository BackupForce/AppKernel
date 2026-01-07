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

        // 改用顯式 handler 參數宣告，避免 Minimal API 自動推斷參數來源。
        group.MapPost(
                "/",
                async (CreateRoleRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateRoleCommand command = new CreateRoleCommand(request.Name);
                    return await UseCaseInvoker.Send<CreateRoleCommand, int>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.Create.Name)
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateRole");

        group.MapPut(
                "/{id:int}",
                async (int id, UpdateRoleRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateRoleCommand command = new UpdateRoleCommand(id, request.Name);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Roles.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateRole");

        group.MapDelete(
                "/{id:int}",
                async (int id, ISender sender, CancellationToken ct) =>
                {
                    DeleteRoleCommand command = new DeleteRoleCommand(id);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Roles.Delete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("DeleteRole");

        group.MapGet(
                "/{id:int}",
                async (int id, ISender sender, CancellationToken ct) =>
                {
                    GetRoleByIdQuery request = new GetRoleByIdQuery(id);
                    return await UseCaseInvoker.Send<GetRoleByIdQuery, RoleDetailDto>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<RoleDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetRoleById");

        group.MapGet(
                "/",
                async (ISender sender, CancellationToken ct) =>
                {
                    ListRolesQuery query = new ListRolesQuery();
                    return await UseCaseInvoker.Send<ListRolesQuery, IReadOnlyList<RoleListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<IReadOnlyList<RoleListItemDto>>(StatusCodes.Status200OK)
            .WithName("ListRoles");

        group.MapGet(
                "/{id:int}/permissions",
                async (int id, ISender sender, CancellationToken ct) =>
                {
                    GetRolePermissionsQuery request = new GetRolePermissionsQuery(id);
                    return await UseCaseInvoker.Send<GetRolePermissionsQuery, IReadOnlyList<string>>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetRolePermissions");

        group.MapPost(
                "/{id:int}/permissions",
                async (int id, UpdateRolePermissionsRequest request, ISender sender, CancellationToken ct) =>
                {
                    AddRolePermissionsCommand command = new AddRolePermissionsCommand(id, request.PermissionCodes);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Roles.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AddRolePermissions");

        group.MapPost(
            "/{id:int}/permissions/remove",
            async (int id, UpdateRolePermissionsRequest request, ISender sender, CancellationToken ct) =>
            {
                RemoveRolePermissionsCommand command = new RemoveRolePermissionsCommand(id, request.PermissionCodes);
                return await UseCaseInvoker.Send(command, sender, ct);
            })
        .RequireAuthorization(Permission.Roles.Update.Name)
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithName("RemoveRolePermissions");
    }
}
