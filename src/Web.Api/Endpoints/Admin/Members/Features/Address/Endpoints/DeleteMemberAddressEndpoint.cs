using Application.Abstractions.Authorization;
using Application.Members.Addresses;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Features.Address.Requests;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Features.Address.Endpoints;

public static class DeleteMemberAddressEndpoint
{
    public static RouteHandlerBuilder MapDeleteMemberAddressEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapDelete(
                "/members/{memberId:guid}/addresses/{id:guid}",
                async (Guid memberId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    var command = new DeleteMemberAddressCommand(memberId, id);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminDeleteMemberAddress");
    }
}
