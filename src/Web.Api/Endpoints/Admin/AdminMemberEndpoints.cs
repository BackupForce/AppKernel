using Application.Abstractions.Authorization;
using Application.Members.Dtos;
using Application.Members.GetById;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminMemberEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Members");

        group.MapGet(
                "/members/{memberId:guid}",
                async (Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    GetMemberByIdQuery query = new GetMemberByIdQuery(memberId);
                    return await UseCaseInvoker.Send<GetMemberByIdQuery, MemberDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<MemberDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberById");
    }
}
