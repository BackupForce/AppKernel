using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Activity.GetActivity;
using Application.Members.Dtos;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetMemberActivityLogEndpoint
{
    public static RouteHandlerBuilder MapGetMemberActivityLogEndpoint(
        this RouteGroupBuilder group)
    {
        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        return group.MapGet(
                "/{id:guid}/activity",
                (Guid id, [AsParameters] MemberActivityRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetMemberActivityLogQuery(
                        id,
                        request.StartDate,
                        request.EndDate,
                        request.Action,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Send<GetMemberActivityLogQuery, PagedResult<MemberActivityLogDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberAudit.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<PagedResult<MemberActivityLogDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberActivityLog");
    }
}
