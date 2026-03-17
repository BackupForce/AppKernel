using Application.Abstractions.Authorization;
using Application.Members.Tags.Create;
using Application.Members.Tags.Update;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.MemberTags.Requests;

namespace Web.Api.Endpoints.Admin.MemberTags.Endpoints;

public static class UpdateMemberTagEndpoint
{
    public static RouteHandlerBuilder MapUpdateMemberTagEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
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
    }
}
