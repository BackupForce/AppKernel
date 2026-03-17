using Application.Abstractions.Authorization;
using Application.Members.Addresses;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Features.Address.Endpoints;

public static class GetMemberAddressesEndpoint
{
    public static RouteHandlerBuilder MapGetMemberAddressesEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/members/{memberId:guid}/addresses",
                async (Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetMemberAddressesQuery(memberId);
                    return await UseCaseInvoker.Send<GetMemberAddressesQuery, IReadOnlyList<MemberAddressDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<IReadOnlyList<MemberAddressDto>>(StatusCodes.Status200OK)
            .WithName("AdminGetMemberAddresses");
    }
}
