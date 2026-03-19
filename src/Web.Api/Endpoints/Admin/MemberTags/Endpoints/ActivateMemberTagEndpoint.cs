using Application.Members.Tags.ActivateMemberTag;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.MemberTags.Endpoints;

public static class ActivateMemberTagEndpoint
{
    public static RouteHandlerBuilder MapActivateMemberTagEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/member-tags/{id:guid}/activate",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    ActivateMemberTagCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.MemberTag.Activate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminActivateMemberTag");
    }
}
