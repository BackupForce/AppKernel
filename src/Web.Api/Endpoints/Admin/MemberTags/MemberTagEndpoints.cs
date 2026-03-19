using Application.Abstractions.Authorization;
using Asp.Versioning;
using Web.Api.Endpoints.Admin.MemberTags.Endpoints;

namespace Web.Api.Endpoints.Admin.MemberTags;

public sealed class MemberTagEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Member Tags");

        group.MapCreateMemberTagEndpoint();
        group.MapUpdateMemberTagEndpoint();
        group.MapDeactivateMemberTagEndpoint();
        group.MapGetMemberTagsEndpoint();
        group.MapAssignMemberTagEndpoint();
        group.MapUnassignMemberTagEndpoint();
    }
}
