using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Update;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class UpdateMemberProfileEndpoint
{
    public static RouteHandlerBuilder MapUpdateMemberProfileEndpoint(
        this RouteGroupBuilder group)
    {
        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        return group.MapPut(
                "/{id:guid}",
                async (Guid id, UpdateMemberProfileRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateMemberProfileCommand(id, request.DisplayName);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateMemberProfile");

    }
}
