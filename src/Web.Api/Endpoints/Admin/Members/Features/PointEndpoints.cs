using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Members.Dtos;
using Application.Members.Points.Adjust;
using Application.Members.Points.GetBalance;
using Application.Members.Points.GetHistory;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Features;

internal static class PointEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/{id:guid}/points")
            .WithTags("Member.Points");

        var memberNodeMetadata = new ResourceNodeMetadata("id", ResourceNodeKeys.MemberPrefix);

        group.MapGet(
                "/balance",
                async (Guid id, ISender sender, CancellationToken ct) =>
                {
                    var request = new GetMemberPointBalanceQuery(id);
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
                "/history",
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
                "/adjust",
                async (Guid id, AdjustMemberPointsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new AdjustMemberPointsCommand(
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

    }
}
