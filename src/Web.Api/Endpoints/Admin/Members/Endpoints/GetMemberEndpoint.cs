using Application.Abstractions.Authorization;
using Application.Members.Dtos;
using Application.Members.GetById;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetMemberEndpoint
{
    public static RouteHandlerBuilder MapGetMemberEndpoint(
        this RouteGroupBuilder group)
    {
        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        return group.MapGet(
                "/{id:guid}",
                async (Guid id, ISender sender, CancellationToken ct) =>
                {
                    var request = new GetMemberByIdQuery(id);
                    return await UseCaseInvoker.Send<GetMemberByIdQuery, MemberDetailDto>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.View.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<MemberDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetMemberById");
    }
}
