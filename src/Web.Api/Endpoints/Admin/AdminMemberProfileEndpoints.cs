using Application.Abstractions.Authorization;
using Application.Members.Profiles;
using Asp.Versioning;
using Domain.Members;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminMemberProfileEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Members");

        group.MapGet(
                "/members/{memberId:guid}/profile",
                async (Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    GetMemberProfileQuery query = new GetMemberProfileQuery(memberId);
                    return await UseCaseInvoker.Send<GetMemberProfileQuery, MemberProfileDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<MemberProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberProfile");

        group.MapPut(
                "/members/{memberId:guid}/profile",
                async (Guid memberId, UpsertMemberProfileRequest request, ISender sender, CancellationToken ct) =>
                {
                    string? realName = string.IsNullOrWhiteSpace(request.RealName) ? null : request.RealName.Trim();
                    string? phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
                    Gender gender = (Gender)request.Gender;
                    UpsertMemberProfileCommand command = new UpsertMemberProfileCommand(
                        memberId,
                        realName,
                        gender,
                        phoneNumber,
                        request.PhoneVerified);

                    return await UseCaseInvoker.Send<UpsertMemberProfileCommand, MemberProfileDto>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces<MemberProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUpsertMemberProfile");
    }
}
