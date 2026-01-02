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
using Web.Api.Common;
using Web.Api.Endpoints.Members.Requests;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Members;

public sealed class MembersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/members")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization()
            .WithTags("Members");

        group.MapPost(
                "/",
                (CreateMemberRequest request, ISender sender, CancellationToken ct) =>
                    UseCaseInvoker.Handle<CreateMemberCommand, Guid>(
                        new CreateMemberCommand(request.UserId, request.DisplayName, request.MemberNo),
                        sender,
                        ct))
            .RequireAuthorization(Permission.Members.Create.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateMember");

        group.MapGet(
                "/{id:guid}",
                UseCaseInvoker.FromRoute<GetMemberByIdQuery, Guid, MemberDetailDto>(id => new GetMemberByIdQuery(id)))
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<MemberDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetMemberById");

        group.MapGet(
                "/",
                ([AsParameters] SearchMembersRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new SearchMembersQuery(
                        request.MemberNo,
                        request.DisplayName,
                        request.Status,
                        request.UserId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Handle<SearchMembersQuery, PagedResult<MemberListItemDto>>(query, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<PagedResult<MemberListItemDto>>(StatusCodes.Status200OK)
            .WithName("SearchMembers");

        group.MapPut(
                "/{id:guid}",
                UseCaseInvoker.FromRoute<UpdateMemberProfileCommand, Guid, UpdateMemberProfileRequest>(
                    (id, request) => new UpdateMemberProfileCommand(id, request.DisplayName)))
            .RequireAuthorization(Permission.Members.Update.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateMemberProfile");

        group.MapPost(
                "/{id:guid}/suspend",
                UseCaseInvoker.FromRoute<SuspendMemberCommand, Guid, MemberStatusChangeRequest>(
                    (id, request) => new SuspendMemberCommand(id, request.Reason)))
            .RequireAuthorization(Permission.Members.Suspend.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SuspendMember");

        group.MapPost(
                "/{id:guid}/activate",
                UseCaseInvoker.FromRoute<ActivateMemberCommand, Guid, MemberStatusChangeRequest>(
                    (id, request) => new ActivateMemberCommand(id, request.Reason)))
            .RequireAuthorization(Permission.Members.Suspend.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateMember");

        group.MapGet(
                "/{id:guid}/points/balance",
                UseCaseInvoker.FromRoute<GetMemberPointBalanceQuery, Guid, MemberPointBalanceDto>(
                    id => new GetMemberPointBalanceQuery(id)))
            .RequireAuthorization(Permission.MemberPoints.Read.Name)
            .Produces<MemberPointBalanceDto>(StatusCodes.Status200OK)
            .WithName("GetMemberPointBalance");

        group.MapGet(
                "/{id:guid}/points/history",
                (Guid id, [AsParameters] MemberPointHistoryRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetMemberPointHistoryQuery(
                        id,
                        request.StartDate,
                        request.EndDate,
                        request.Type,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Handle<GetMemberPointHistoryQuery, PagedResult<MemberPointLedgerDto>>(
                        query,
                        sender,
                        ct);
                })
            .RequireAuthorization(Permission.MemberPoints.Read.Name)
            .Produces<PagedResult<MemberPointLedgerDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberPointHistory");

        group.MapPost(
                "/{id:guid}/points/adjust",
                UseCaseInvoker.FromRoute<AdjustMemberPointsCommand, Guid, AdjustMemberPointsRequest, long>(
                    (id, request) => new AdjustMemberPointsCommand(
                        id,
                        request.Delta,
                        request.Remark,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.AllowNegative)))
            .RequireAuthorization(Permission.MemberPoints.Adjust.Name)
            .Produces<long>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdjustMemberPoints");

        group.MapGet(
                "/{id:guid}/assets",
                UseCaseInvoker.FromRoute<GetMemberAssetsQuery, Guid, IReadOnlyCollection<MemberAssetBalanceDto>>(
                    id => new GetMemberAssetsQuery(id)))
            .RequireAuthorization(Permission.MemberAssets.Read.Name)
            .Produces<IReadOnlyCollection<MemberAssetBalanceDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberAssets");

        group.MapGet(
                "/{id:guid}/assets/{assetCode}/history",
                (Guid id, string assetCode, [AsParameters] MemberAssetHistoryRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetMemberAssetHistoryQuery(
                        id,
                        assetCode,
                        request.StartDate,
                        request.EndDate,
                        request.Type,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Handle<GetMemberAssetHistoryQuery, PagedResult<MemberAssetLedgerDto>>(
                        query,
                        sender,
                        ct);
                })
            .RequireAuthorization(Permission.MemberAssets.Read.Name)
            .Produces<PagedResult<MemberAssetLedgerDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberAssetHistory");

        group.MapPost(
                "/{id:guid}/assets/adjust",
                UseCaseInvoker.FromRoute<AdjustMemberAssetCommand, Guid, AdjustMemberAssetRequest, decimal>(
                    (id, request) => new AdjustMemberAssetCommand(
                        id,
                        request.AssetCode,
                        request.Delta,
                        request.Remark,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.AllowNegative)))
            .RequireAuthorization(Permission.MemberAssets.Adjust.Name)
            .Produces<decimal>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdjustMemberAssets");

        group.MapGet(
                "/{id:guid}/activity",
                (Guid id, [AsParameters] MemberActivityRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetMemberActivityLogQuery(
                        id,
                        request.StartDate,
                        request.EndDate,
                        request.Action,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Handle<GetMemberActivityLogQuery, PagedResult<MemberActivityLogDto>>(
                        query,
                        sender,
                        ct);
                })
            .RequireAuthorization(Permission.MemberAudit.Read.Name)
            .Produces<PagedResult<MemberActivityLogDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberActivityLog");
    }
}
