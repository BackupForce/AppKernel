using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Profiles;
using Application.Members.Tags.GetMemberTags;
using Application.Members.Update;
using Domain.Members;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetMemberTagsEndpoint
{
    public static RouteHandlerBuilder MapGetMemberTagsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{memberId:guid}/tags",
                async (Guid tenantId, Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    GetMemberTagsQuery query = new(tenantId, memberId);
                    return await UseCaseInvoker.Send<GetMemberTagsQuery, IReadOnlyCollection<MemberTagDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.View.Name)
            .Produces<IReadOnlyCollection<MemberTagDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberTags");
    }
}
