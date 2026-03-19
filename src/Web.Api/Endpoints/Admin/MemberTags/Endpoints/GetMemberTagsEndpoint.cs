using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Tags.Create;
using Application.Members.Tags.List;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.MemberTags.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.MemberTags.Endpoints;

public static class GetMemberTagsEndpoint
{
    public static RouteHandlerBuilder MapGetMemberTagsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
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
            .RequireAuthorization(Permission.MemberTag.Read.Name)
            .Produces<PagedResult<MemberTagDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminListMemberTags");
    }
}
