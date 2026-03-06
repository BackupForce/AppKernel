using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Tags.Create;
using Application.Members.Tags.Deactivate;
using Application.Members.Tags.GetMemberTags;
using Application.Members.Tags.List;
using Application.Members.Tags.ReplaceMemberTags;
using Application.Members.Tags.Update;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminMemberTagEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Member Tags");

        group.MapPost(
                "/member-tags",
                async (Guid tenantId, CreateMemberTagRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateMemberTagCommand command = new(tenantId, request.TagCode, request.DisplayName);
                    return await UseCaseInvoker.Send<CreateMemberTagCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminCreateMemberTag");

        group.MapPut(
                "/member-tags/{tagId:guid}",
                async (Guid tenantId, Guid tagId, UpdateMemberTagRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateMemberTagCommand command = new(tenantId, tagId, request.DisplayName);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUpdateMemberTag");

        group.MapPost(
                "/member-tags/{tagId:guid}/deactivate",
                async (Guid tenantId, Guid tagId, ISender sender, CancellationToken ct) =>
                {
                    DeactivateMemberTagCommand command = new(tenantId, tagId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminDeactivateMemberTag");

        group.MapGet(
                "/member-tags",
                async ([AsParameters] ListMemberTagsRequest request, Guid tenantId, ISender sender, CancellationToken ct) =>
                {
                    if (request.Page < 1)
                    {
                        return Results.BadRequest("Page must be greater than or equal to 1.");
                    }

                    if (request.PageSize < 1 || request.PageSize > 200)
                    {
                        return Results.BadRequest("PageSize must be between 1 and 200.");
                    }

                    ListMemberTagsQuery query = new(tenantId, request.Keyword, request.IsActive, request.Page, request.PageSize);
                    return await UseCaseInvoker.Send<ListMemberTagsQuery, PagedResult<MemberTagDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<PagedResult<MemberTagDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminListMemberTags");

        group.MapGet(
                "/members/{memberId:guid}/tags",
                async (Guid tenantId, Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    GetMemberTagsQuery query = new(tenantId, memberId);
                    return await UseCaseInvoker.Send<GetMemberTagsQuery, IReadOnlyCollection<MemberTagDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<IReadOnlyCollection<MemberTagDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberTags");

        group.MapPut(
                "/members/{memberId:guid}/tags",
                async (Guid tenantId, Guid memberId, ReplaceMemberTagsRequest request, ISender sender, CancellationToken ct) =>
                {
                    ReplaceMemberTagsCommand command = new(tenantId, memberId, request.TagIds ?? Array.Empty<Guid>());
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminReplaceMemberTags");
    }
}
