using Application.Abstractions.Authorization;
using Application.Members.Tags.Deactivate;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.MemberTags.Endpoints;

public static class DeactivateMemberTagEndpoint
{
    public static RouteHandlerBuilder MapDeactivateMemberTagEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/member-tags/{tagId:guid}/deactivate",
                async (Guid tenantId, Guid tagId, ISender sender, CancellationToken ct) =>
                {
                    DeactivateMemberTagCommand command = new(tenantId, tagId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.MemberTag.Delete.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminDeactivateMemberTag");
    }
}
