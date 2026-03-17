using Application.Abstractions.Authorization;
using Application.Members.Addresses;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Features.Address.Requests;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Features.Address.Endpoints;

public static class UpdateMemberAddressEndpoint
{
    public static RouteHandlerBuilder MapUpdateMemberAddressEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/members/{memberId:guid}/addresses/{id:guid}",
                async (Guid memberId, Guid id, UpdateMemberAddressRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdateMemberAddressCommand(
                        memberId,
                        id,
                        request.ReceiverName,
                        request.PhoneNumber,
                        request.Country,
                        request.City,
                        request.District,
                        request.AddressLine,
                        request.IsDefault);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUpdateMemberAddress");
    }
}
