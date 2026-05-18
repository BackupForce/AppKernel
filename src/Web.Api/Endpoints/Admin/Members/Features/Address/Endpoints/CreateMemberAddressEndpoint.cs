using Application.Abstractions.Authorization;
using Application.Members.Addresses;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Features.Address.Requests;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Features.Address.Endpoints;

public static class CreateMemberAddressEndpoint
{
    public static RouteHandlerBuilder MapCreateMemberAddressEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/members/{memberId:guid}/addresses",
                async (Guid memberId, CreateMemberAddressRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreateMemberAddressCommand(
                        memberId,
                        request.ReceiverName,
                        request.PhoneNumber,
                        request.Country,
                        request.City,
                        request.District,
                        request.AddressLine,
                        request.IsDefault);
                    return await UseCaseInvoker.Send<CreateMemberAddressCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminCreateMemberAddress");
    }
}
