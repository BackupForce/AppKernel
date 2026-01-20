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
using Web.Api.Endpoints.Members.Requests;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Members;

public sealed class MembersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/members")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Members");

        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        // 改用顯式 handler 參數宣告，避免 Minimal API 自動推斷 route/body/service 造成例外。
        group.MapPost(
                "/",
                async (CreateMemberRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateMemberCommand command = new CreateMemberCommand(request.UserId, request.DisplayName, request.MemberNo);
                    return await UseCaseInvoker.Send<CreateMemberCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Create.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateMember");

        group.MapGet(
                "/{id:guid}",
                async (Guid id, ISender sender, CancellationToken ct) =>
                {
                    GetMemberByIdQuery request = new GetMemberByIdQuery(id);
                    return await UseCaseInvoker.Send<GetMemberByIdQuery, MemberDetailDto>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<MemberDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetMemberById");

        group.MapGet(
                "/",
                ([AsParameters] SearchMembersRequest request, ISender sender, CancellationToken ct) =>
                {
                    SearchMembersQuery query = new SearchMembersQuery(
                        request.MemberNo,
                        request.DisplayName,
                        request.Status,
                        request.UserId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Send<SearchMembersQuery, PagedResult<MemberListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Read.Name)
            .Produces<PagedResult<MemberListItemDto>>(StatusCodes.Status200OK)
            .WithName("SearchMembers");

        group.MapPut(
                "/{id:guid}",
                async (Guid id, UpdateMemberProfileRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateMemberProfileCommand command = new UpdateMemberProfileCommand(id, request.DisplayName);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Update.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateMemberProfile");

        group.MapPost(
                "/{id:guid}/suspend",
                async (Guid id, MemberStatusChangeRequest request, ISender sender, CancellationToken ct) =>
                {
                    SuspendMemberCommand command = new SuspendMemberCommand(id, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Suspend.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SuspendMember");

        group.MapPost(
                "/{id:guid}/activate",
                async (Guid id, MemberStatusChangeRequest request, ISender sender, CancellationToken ct) =>
                {
                    ActivateMemberCommand command = new ActivateMemberCommand(id, request.Reason);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Members.Suspend.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("ActivateMember");

        group.MapGet(
                "/{id:guid}/points/balance",
                async (Guid id, ISender sender, CancellationToken ct) =>
                {
                    GetMemberPointBalanceQuery request = new GetMemberPointBalanceQuery(id);
                    return await UseCaseInvoker.Send<GetMemberPointBalanceQuery, MemberPointBalanceDto>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberPoints.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<MemberPointBalanceDto>(StatusCodes.Status200OK)
            .WithName("GetMemberPointBalance");

        group.MapGet(
                "/{id:guid}/points/history",
                (Guid id, [AsParameters] MemberPointHistoryRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMemberPointHistoryQuery query = new GetMemberPointHistoryQuery(
                        id,
                        request.StartDate,
                        request.EndDate,
                        request.Type,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Send<GetMemberPointHistoryQuery, PagedResult<MemberPointLedgerDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberPoints.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<PagedResult<MemberPointLedgerDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberPointHistory");

        group.MapPost(
                "/{id:guid}/points/adjust",
                async (Guid id, AdjustMemberPointsRequest request, ISender sender, CancellationToken ct) =>
                {
                    AdjustMemberPointsCommand command = new AdjustMemberPointsCommand(
                        id,
                        request.Delta,
                        request.Remark,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.AllowNegative);
                    return await UseCaseInvoker.Send<AdjustMemberPointsCommand, long>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberPoints.Adjust.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<long>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdjustMemberPoints");

        group.MapGet(
                "/{id:guid}/assets",
                async (Guid id, ISender sender, CancellationToken ct) =>
                {
                    GetMemberAssetsQuery request = new GetMemberAssetsQuery(id);
                    return await UseCaseInvoker.Send<GetMemberAssetsQuery, IReadOnlyCollection<MemberAssetBalanceDto>>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberAssets.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<IReadOnlyCollection<MemberAssetBalanceDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberAssets");

        group.MapGet(
                "/{id:guid}/assets/{assetCode}/history",
                (Guid id, string assetCode, [AsParameters] MemberAssetHistoryRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMemberAssetHistoryQuery query = new GetMemberAssetHistoryQuery(
                        id,
                        assetCode,
                        request.StartDate,
                        request.EndDate,
                        request.Type,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Send<GetMemberAssetHistoryQuery, PagedResult<MemberAssetLedgerDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberAssets.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<PagedResult<MemberAssetLedgerDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberAssetHistory");

        group.MapPost(
                "/{id:guid}/assets/adjust",
                async (Guid id, AdjustMemberAssetRequest request, ISender sender, CancellationToken ct) =>
                {
                    AdjustMemberAssetCommand command = new AdjustMemberAssetCommand(
                        id,
                        request.AssetCode,
                        request.Delta,
                        request.Remark,
                        request.ReferenceType,
                        request.ReferenceId,
                        request.AllowNegative);
                    return await UseCaseInvoker.Send<AdjustMemberAssetCommand, decimal>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberAssets.Adjust.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<decimal>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdjustMemberAssets");

        group.MapGet(
                "/{id:guid}/activity",
                (Guid id, [AsParameters] MemberActivityRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMemberActivityLogQuery query = new GetMemberActivityLogQuery(
                        id,
                        request.StartDate,
                        request.EndDate,
                        request.Action,
                        request.Page,
                        request.PageSize);

                    return UseCaseInvoker.Send<GetMemberActivityLogQuery, PagedResult<MemberActivityLogDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberAudit.Read.Name)
            .WithMetadata(memberNodeMetadata)
            .Produces<PagedResult<MemberActivityLogDto>>(StatusCodes.Status200OK)
            .WithName("GetMemberActivityLog");
    }
}
