using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claim;
using Application.Gaming.Tickets.AvailableForBet;
using Application.Gaming.Tickets.GetMy;
using Application.Gaming.Tickets.GetMyWinningTickets;
using Application.Gaming.Tickets.Place;
using Application.Gaming.Tickets.Redeem;
using Application.Gaming.Tickets.Submit;
using Domain.Gaming.Shared;
using Domain.Members;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Frontends.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Frontends.SubEndpoints;

internal static class TicketEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/tickets")
            .WithTags("FE.Tickets");


        group.MapGet(
                "/available-for-bet",
                async ([AsParameters] GetAvailableTicketsForBetRequest request,
                    IMemberRepository memberRepository,
                    ITenantContext tenantContext,
                    IUserContext userContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    Member? member = await memberRepository.GetByUserIdAsync(
                        tenantContext.TenantId,
                        userContext.UserId,
                        ct);
                    if (member is null)
                    {
                        return CustomResults.Problem(Result.Failure(GamingErrors.MemberNotFound));
                    }

                    GetAvailableTicketsForBetQuery query = new GetAvailableTicketsForBetQuery(
                        tenantContext.TenantId,
                        member.Id,
                        request.DrawId,
                        request.Limit);
                    return await UseCaseInvoker.Send<GetAvailableTicketsForBetQuery, AvailableTicketsResponse>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<AvailableTicketsResponse>(StatusCodes.Status200OK)
            .WithSummary("可供參與票券")
            .WithDescription("取得未參與票券清單")
            .WithName("GetAvailableTicketsForBet");

        group.MapGet(
                "/",
                async ([AsParameters] GetMyTicketsRequest request, ISender sender, CancellationToken ct) =>
                {
                    GetMyTicketsQuery query = new(
                        request.GameCode,
                        request.From,
                        request.To,
                        request.PageNumber,
                        request.PageSize);

                    return await UseCaseInvoker.Send<GetMyTicketsQuery, PagedResult<TicketSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<PagedResult<TicketSummaryDto>>(StatusCodes.Status200OK)
            .WithSummary("會員票券查詢")
            .WithDescription("依遊戲代碼與時間區間查詢會員票券，並回傳分頁結果。")
            .WithName("GetMyGameTickets");


        //group.MapPost(
        //        "/",
        //        async (PlaceTicketRequest request, ISender sender, CancellationToken ct) =>
        //        {
        //            var command = new PlaceTicketCommand(
        //                request.DrawId,
        //                request.PlayTypeCode,
        //                request.TemplateId,
        //                request.Lines);
        //            return await UseCaseInvoker.Send<PlaceTicketCommand, Guid>(
        //                command,
        //                sender,
        //                value => Results.Ok(value),
        //                ct);
        //        })
        //    .RequireAuthorization(AuthorizationPolicyNames.Member)
        //    .Produces<Guid>(StatusCodes.Status200OK)
        //    .ProducesProblem(StatusCodes.Status400BadRequest)
        //    .WithName("PlaceGameTicket");


        group.MapPost(
                "/{ticketId:guid}/submit",
                async (Guid ticketId, SubmitTicketNumbersRequest request, ISender sender, CancellationToken ct) =>
                {
                    SubmitTicketNumbersCommand command =
                        new(ticketId, request.PlayTypeCode, request.Numbers);

                    return await UseCaseInvoker.Send(
                        command,
                        sender,
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("SubmitTicketNumbers");

        group.MapGet(
               "/winnings",
               async (
                   [FromQuery] int pageNumber,
                   [FromQuery] int pageSize,
                   ISender sender,
                   CancellationToken ct) =>
               {
                   GetMyWinningTicketsQuery query = new GetMyWinningTicketsQuery(pageNumber, pageSize);
                   return await UseCaseInvoker.Send<GetMyWinningTicketsQuery, PagedResult<MyWinningTicketItemDto>>(
                       query,
                       sender,
                       value => Results.Ok(value),
                       ct);
               })
           .RequireAuthorization(AuthorizationPolicyNames.Member)
           .Produces<PagedResult<MyWinningTicketItemDto>>(StatusCodes.Status200OK)
           .WithSummary("中獎票券")
           .WithDescription("取得會員中獎票券清單")
           .WithName("GetMyWinningTickets");

        group.MapPost(
                "/{ticketLineResultId:guid}/redeem",
                async (Guid ticketLineResultId, ISender sender, CancellationToken ct) =>
                {
                    RedeemTicketLineCommand command = new RedeemTicketLineCommand(ticketLineResultId);
                    return await UseCaseInvoker.Send<RedeemTicketLineCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(new { id = value }),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("中獎兌獎")
            .WithDescription("兌換指定中獎票券明細")
            .WithName("RedeemWinningTicketLine");

    }

}
