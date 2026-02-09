using Application.Abstractions.Authorization;
using Application.Users.RemoveRole;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminUserRoleEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Users");

        group.MapDelete(
                "/users/{userId:guid}/roles/{roleName}",
                async (Guid userId, string roleName, ISender sender, CancellationToken ct) =>
                {
                    RemoveUserRoleCommand command = new RemoveUserRoleCommand(userId, roleName);
                    Result<RemoveUserRoleResultDto> result = await sender.Send(command, ct);
                    return result.Match(
                        _ => Results.NoContent(),
                        failure => CustomResults.Problem(failure));
                })
            .RequireAuthorization(Permission.Users.Update.Name)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("RemoveUserRole");
    }
}
