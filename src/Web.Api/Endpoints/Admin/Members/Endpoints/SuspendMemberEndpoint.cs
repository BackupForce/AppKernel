using Application.Abstractions.Authorization;
using Application.Members.Suspend;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class SuspendMemberEndpoint
{
    public static RouteHandlerBuilder MapSuspendMemberEndpoint(
        this RouteGroupBuilder group)
    {
        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        return group.MapPost(
                "/{id:guid}/suspend",
                async (Guid id, MemberStatusChangeRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new SuspendMemberCommand(id, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Suspend.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SuspendMember");

    }
}
