using Application.Abstractions.Authorization;
using Application.Members.Addresses;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminMemberAddressEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Members");

        group.MapGet(
                "/members/{memberId:guid}/addresses",
                async (Guid memberId, ISender sender, CancellationToken ct) =>
                {
                    GetMemberAddressesQuery query = new GetMemberAddressesQuery(memberId);
                    return await UseCaseInvoker.Send<GetMemberAddressesQuery, IReadOnlyList<MemberAddressDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<IReadOnlyList<MemberAddressDto>>(StatusCodes.Status200OK)
            .WithName("AdminGetMemberAddresses");

        group.MapPost(
                "/members/{memberId:guid}/addresses",
                async (Guid memberId, CreateMemberAddressRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateMemberAddressCommand command = new CreateMemberAddressCommand(
                        memberId,
                        request.ReceiverName,
                        request.PhoneNumber,
                        request.Country,
                        request.City,
                        request.District,
                        request.AddressLine,
                        request.IsDefault);
                    return await UseCaseInvoker.Send<CreateMemberAddressCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminCreateMemberAddress");

        group.MapPut(
                "/members/{memberId:guid}/addresses/{id:guid}",
                async (Guid memberId, Guid id, UpdateMemberAddressRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateMemberAddressCommand command = new UpdateMemberAddressCommand(
                        memberId,
                        id,
                        request.ReceiverName,
                        request.PhoneNumber,
                        request.Country,
                        request.City,
                        request.District,
                        request.AddressLine,
                        request.IsDefault);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUpdateMemberAddress");

        group.MapDelete(
                "/members/{memberId:guid}/addresses/{id:guid}",
                async (Guid memberId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    DeleteMemberAddressCommand command = new DeleteMemberAddressCommand(memberId, id);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminDeleteMemberAddress");

        group.MapPost(
                "/members/{memberId:guid}/addresses/{id:guid}/set-default",
                async (Guid memberId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    SetDefaultMemberAddressCommand command = new SetDefaultMemberAddressCommand(memberId, id);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminSetMemberAddressDefault");
    }
}
