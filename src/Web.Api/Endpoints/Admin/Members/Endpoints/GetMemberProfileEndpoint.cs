using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Profiles;
using Application.Members.Update;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetMemberProfileEndpoint
{
    public static RouteHandlerBuilder MapGetMemberProfileEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/members/{memberId:guid}/profile",
                async (Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    GetMemberProfileQuery query = new GetMemberProfileQuery(memberId);
                    return await UseCaseInvoker.Send<GetMemberProfileQuery, MemberProfileDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<MemberProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberProfile");
    }
}
