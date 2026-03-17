using Application.Abstractions.Authorization;
using Application.Members.Activate;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class ActivateMemberEndpoint
{
    public static RouteHandlerBuilder MapActivateMemberEndpoint(
        this RouteGroupBuilder group)
    {
        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        return group.MapPost(
                "/{id:guid}/activate",
                async (Guid id, MemberStatusChangeRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new ActivateMemberCommand(id, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Suspend.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateMember");
    }
}
