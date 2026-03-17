using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Activate;
using Application.Members.Activity.GetActivity;
using Application.Members.Assets.Adjust;
using Application.Members.Assets.GetAssets;
using Application.Members.Assets.GetHistory;
using Application.Members.Create;
using Application.Members.Dtos;
using Application.Members.GetById;
using Application.Members.Points.Adjust;
using Application.Members.Points.GetBalance;
using Application.Members.Points.GetHistory;
using Application.Members.Search;
using Application.Members.Suspend;
using Application.Members.Update;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Endpoints;
using Web.Api.Endpoints.Admin.Members.Features.Address;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Admin.Members;

public sealed class MembersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/members")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Members");

        //var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        group.MapGetMemberEndpoint();
        group.MapGetMembersEndpoint();
        group.MapCreateMemberEndpoint();
        group.MapUpdateMemberProfileEndpoint();
        group.MapActivateMemberEndpoint();
        group.MapSuspendMemberEndpoint();
        
        group.MapGetMemberActivityLogEndpoint();

        group.MapGetMemberProfileEndpoint();
        group.MapUpsertMemberProfileEndpoint();

        //Tags
        group.MapGetMemberTagsEndpoint();
        group.MapReplaceMemberTagsEndpoint();

        //Tickets
        group.MapIssueMemberTicketsEndpoint();
        group.MapGetAvailableTicketsForBetEndpoint();

        MemberAddressEndpoints.Map(group);
    }
}
