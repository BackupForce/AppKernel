using Application.Abstractions.Authorization;
using Application.Users.GetTenantUsers;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Web.Api.Common;

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
                async (Guid tenantId, ISender sender, CancellationToken ct) =>
                {
                    GetTenantUsersQuery query = new GetTenantUsersQuery(tenantId);
                    return await UseCaseInvoker.Send<GetTenantUsersQuery, IReadOnlyList<TenantUserListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Users.View.Name)
            .Produces<IReadOnlyList<TenantUserListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithName("GetTenantUsers");
    }
}
