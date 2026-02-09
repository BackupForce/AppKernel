using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Users.GetTenantUsers;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Users.Requests;

namespace Web.Api.Endpoints.Users;

public sealed class TenantUsersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/users")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Users");

        group.MapGet(
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
