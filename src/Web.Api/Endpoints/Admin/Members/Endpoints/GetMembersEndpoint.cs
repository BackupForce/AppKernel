using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Search;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetMembersEndpoint
{
    public static RouteHandlerBuilder MapGetMembersEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                ([AsParameters] SearchMembersRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new SearchMembersQuery(
                        request.Keyword,
                        request.MemberNo,
                        request.DisplayName,
                        request.PhoneNumber,
                        request.Status,
                        request.UserId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Send<SearchMembersQuery, PagedResult<MemberListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.View.Name)
            .Produces<PagedResult<MemberListItemDto>>(StatusCodes.Status200OK)
            .WithSummary("Search members")
            .WithDescription("Supports filtering by member no, display name, status, user id, keyword (name/email/phone), and phone number.")
            .WithName("SearchMembers");

    }
}
