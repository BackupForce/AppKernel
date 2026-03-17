using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Assets.Adjust;
using Application.Members.Assets.GetAssets;
using Application.Members.Assets.GetHistory;
using Application.Members.Dtos;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Features;

internal static class AssetEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/{id:guid}/points")
            .WithTags("Member.Points");
        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        group.MapGet(
                "/{id:guid}/assets",
                async (Guid id, ISender sender, CancellationToken ct) =>
                {
                    var request = new GetMemberAssetsQuery(id);
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
                    var command = new AdjustMemberAssetCommand(
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

    }
}
