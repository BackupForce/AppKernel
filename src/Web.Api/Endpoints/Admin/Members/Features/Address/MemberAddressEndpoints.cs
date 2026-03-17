using Application.Abstractions.Authorization;
using Application.Members.Addresses;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Features.Address.Endpoints;
using Web.Api.Endpoints.Admin.Members.Features.Address.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin.Members.Features.Address;


internal static class MemberAddressEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/")
            .WithTags("Admin.Members.MemberAddress");

        group.MapCreateMemberAddressEndpoint();
        group.MapUpdateMemberAddressEndpoint();
        group.MapDeleteMemberAddressEndpoint();
        group.MapSetDefaultMemberAddressEndpoint();
        group.MapGetMemberAddressesEndpoint();
    }
}

//public sealed class MemberAddressEndpoints : IEndpoint
//{
//    public void MapEndpoint(IEndpointRouteBuilder app)
//    {
//        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
//            .WithGroupName("admin-v1")
//            .WithMetadata(new ApiVersion(1, 0))
//            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
//            .WithTags("Admin Members");

//        group.MapCreateMemberAddressEndpoint();
//        group.MapUpdateMemberAddressEndpoint();
//        group.MapDeleteMemberAddressEndpoint();
//        group.MapSetDefaultMemberAddressEndpoint();
//        group.MapGetMemberAddressesEndpoint();
//    }
//}
