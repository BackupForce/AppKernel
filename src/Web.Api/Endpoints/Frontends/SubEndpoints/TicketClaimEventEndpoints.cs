using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claim;
using Application.Gaming.TicketClaimEvents.GetActiveTicketClaimEventsForMember;
using Domain.Gaming.Shared;
using Domain.Members;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Common;

namespace Web.Api.Endpoints.Frontends.SubEndpoints;

internal static class TicketClaimEventEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/ticket-claim-events")
            .WithTags("FE.TicketClaimEvents");

        group.MapPost("/{eventId:guid}/claim",
            async (Guid eventId,
                [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                ISender sender,
                CancellationToken ct) =>
            {
                ClaimTicketFromEventCommand command = new(eventId, idempotencyKey);
                return await UseCaseInvoker.Send<ClaimTicketFromEventCommand, TicketClaimResult>(
                    command,
                    sender,
                    value => Results.Ok(value),
                    ct);
            })
        .Produces<TicketClaimResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("活動領票")
        .WithDescription("根據活動Id領取活動票據")
        .WithName("ClaimTicketFromEvent");

        group.MapGet(
                "/active",
                async (IMemberRepository memberRepository,
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

                    GetActiveTicketClaimEventsForMemberQuery query = new(tenantContext.TenantId, member.Id);
                    return await UseCaseInvoker.Send<
                        GetActiveTicketClaimEventsForMemberQuery,
                        IReadOnlyCollection<TicketClaimEventActiveListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .Produces<IReadOnlyCollection<TicketClaimEventActiveListItemDto>>(StatusCodes.Status200OK)
            .WithSummary("取得可參加的活動清單")
            .WithDescription("取得目前仍在活動期間內的 TicketClaimEvent 清單，並標示會員是否仍可參加")
            .WithName("GetActiveTicketClaimEventsForMember");
    }
}
