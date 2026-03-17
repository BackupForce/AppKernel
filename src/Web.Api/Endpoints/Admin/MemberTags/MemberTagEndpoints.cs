using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Tags.Create;
using Application.Members.Tags.Deactivate;
using Application.Members.Tags.GetMemberTags;
using Application.Members.Tags.List;
using Application.Members.Tags.ReplaceMemberTags;
using Application.Members.Tags.Update;
using Asp.Versioning;
using Domain.Security;
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
    }
}
