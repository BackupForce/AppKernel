using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Profiles;
using Application.Members.Tags.GetMemberTags;
using Application.Members.Tags.ReplaceMemberTags;
using Application.Members.Update;
using Domain.Members;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class ReplaceMemberTagsEndpoint
{
    public static RouteHandlerBuilder MapReplaceMemberTagsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{memberId:guid}/tags",
                async (Guid tenantId, Guid memberId, ReplaceMemberTagsRequest request, ISender sender, CancellationToken ct) =>
                {
                    ReplaceMemberTagsCommand command = new(tenantId, memberId, request.TagIds ?? Array.Empty<Guid>());
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.MemberTag.Assign.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminReplaceMemberTags");
    }
}
