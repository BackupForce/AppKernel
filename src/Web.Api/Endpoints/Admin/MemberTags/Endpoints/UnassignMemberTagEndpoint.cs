using Application.Members.Dtos;
using Application.Members.Tags.GetMemberTags;
using Application.Members.Tags.ReplaceMemberTags;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.MemberTags.Requests;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Admin.MemberTags.Endpoints;

public static class UnassignMemberTagEndpoint
{
    public static RouteHandlerBuilder MapUnassignMemberTagEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/member-tags/{tagId:guid}/unassign",
                async (Guid tenantId, Guid tagId, MemberTagAssignmentRequest request, ISender sender, CancellationToken ct) =>
                {
                    Result<IReadOnlyCollection<MemberTagDto>> currentTagsResult = await sender.Send(
                        new GetMemberTagsQuery(tenantId, request.MemberId),
                        ct);

                    if (currentTagsResult.IsFailure)
                    {
                        return UseCaseInvoker.ToIResult(currentTagsResult, Results.Ok);
                    }

                    Guid[] nextTagIds = currentTagsResult.Value
                        .Select(tag => tag.Id)
                        .Where(existingTagId => existingTagId != tagId)
                        .ToArray();

                    return await UseCaseInvoker.Send(
                        new ReplaceMemberTagsCommand(tenantId, request.MemberId, nextTagIds),
                        sender,
                        ct);
                })
            .RequireAuthorization(Permission.MemberTag.Unassign.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUnassignMemberTag");
    }
}
