using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Profiles;
using Application.Members.Update;
using Domain.Members;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class UpsertMemberProfileEndpoint
{
    public static RouteHandlerBuilder MapUpsertMemberProfileEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
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
