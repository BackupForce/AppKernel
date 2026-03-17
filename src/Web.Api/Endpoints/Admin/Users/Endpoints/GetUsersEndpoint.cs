using Application.Abstractions.Data;
using Application.Users.GetTenantUsers;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Users.Requests;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class GetUsersEndpoint
{
    public static RouteHandlerBuilder MapGetUsersEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] GetTenantUsersRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetTenantUsersQuery query = new GetTenantUsersQuery(tenantId, request.Page, request.PageSize);
                    return await UseCaseInvoker.Send<GetTenantUsersQuery, PagedResult<TenantUserListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.View.Name)
            .Produces<PagedResult<TenantUserListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("GetTenantUsers");
    }
}
